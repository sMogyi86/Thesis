using MARGO.BL.Graph;
using MARGO.BL.Img;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MARGO.BL
{
    public class Project
    {
        #region Services
        private readonly IIOService myIOService = Services.GetIO();
        private readonly IProcessingFunctions myProcessingFunctions = Services.GetProcessingFunctions();
        #endregion

        private readonly Runner myRunner = new Runner();
        public byte LevelOfParallelism { get; set; } = (byte)Environment.ProcessorCount;

        public static Project Instance { get; } = new Project();

        private Dictionary<string, RasterLayer> myOriginalLayers = new Dictionary<string, RasterLayer>();
        private Dictionary<string, RasterLayer> myCutedLayers = new Dictionary<string, RasterLayer>();
        private Dictionary<string, RasterLayer> mySampleLayers = new Dictionary<string, RasterLayer>();
        public IEnumerable<RasterLayer> Layers => myOriginalLayers.Values
                                            .Concat(myCutedLayers.Values)
                                            .Concat(mySampleLayers.Values);
        public Variants<int> RAW { get; private set; }
        public Variants<byte> BYTES { get; private set; }
        public Variants<byte> LOGGED { get; private set; }
        public ReadOnlyMemory<byte> MINIMAS { get; private set; }
        private IEnumerable<int> myMinimasIdxs = Enumerable.Empty<int>();

        private IEnumerable<IMST> mySegments = Enumerable.Empty<IMST>();


        private Project() { }



        public void Load(IEnumerable<string> ids)
        {
            if (ids is null || !ids.Any())
                throw new ArgumentNullException(nameof(ids));

            myOriginalLayers = new Dictionary<string, RasterLayer>(ids.Count());
            foreach (var id in ids)
                myOriginalLayers[id] = myIOService.Load(id);

            myCutedLayers = new Dictionary<string, RasterLayer>(myOriginalLayers.Count);
        }

        public async Task Save(Image image, string id) => await myIOService.Save(image, id).ConfigureAwait(false);

        public void Cut(int topLeftX, int topLeftY, int bottomRightX, int bottomRightY, string prefix)
        {
            foreach (var layer in myOriginalLayers.Values)
            {
                var clID = $"{(string.IsNullOrWhiteSpace(prefix) ? nameof(myProcessingFunctions.Cut) : prefix)}_{layer.ID}";
                myCutedLayers[clID] = myProcessingFunctions.Cut(layer, topLeftX, topLeftY, bottomRightX, bottomRightY, clID);
            }
        }

        public bool CanCalcVariants => myCutedLayers.Any();
        public async Task CalculateVariantsWithStatsAsync(byte range)
        {
            var firstLayer = myCutedLayers.First().Value;
            RAW = new Variants<int>(firstLayer.Width, firstLayer.Height);
            var offsetValues = Offsets.CalculateOffsetsFor(firstLayer.Width, range);

            await myRunner.RunAsync(myCutedLayers.First().Value.Memory.Length, LevelOfParallelism,
                (start, length) =>
                {
                    foreach (var layer in myCutedLayers.Values)
                        myProcessingFunctions.CalculateVariants(layer.Memory, RAW.Data, offsetValues, start, length);
                })
                .ContinueWith((t) =>
                {
                    myProcessingFunctions.PopulateStats(RAW);

                    myProcessingFunctions.ReclassToByte(RAW, BYTES = new Variants<byte>(RAW.Width, RAW.Height));
                    myProcessingFunctions.PopulateStats(BYTES);

                    myProcessingFunctions.ReclassToByteLog(RAW, LOGGED = new Variants<byte>(RAW.Width, RAW.Height));
                    myProcessingFunctions.PopulateStats(LOGGED);
                }, CancellationToken.None, TaskContinuationOptions.RunContinuationsAsynchronously, TaskScheduler.Default)
                .ConfigureAwait(false);
        }

        public async Task FindMinimasAsync(byte range)
        {
            var offsetValues = Offsets.CalculateOffsetsFor(LOGGED.Width, range);

            var resultMinimas = await myRunner.PerformAsync(LOGGED.Memory.Length, LevelOfParallelism,
                                    (start, length) =>
                                    {
                                        var listMins = new List<int>();
                                        myProcessingFunctions.FindMinimas(LOGGED.Memory, listMins, offsetValues, start, length);
                                        return listMins;
                                    }).ConfigureAwait(false);

            var minimaIds = new List<int>(resultMinimas.Select(lst => lst.Count).Sum());
            foreach (var listMins in resultMinimas)
                minimaIds.AddRange(listMins);

            myMinimasIdxs = minimaIds;

            var minimas = new byte[LOGGED.Memory.Length];
            foreach (var idx in myMinimasIdxs)
                minimas[idx] = byte.MaxValue;

            MINIMAS = minimas;
        }

        public bool CanFlood => myMinimasIdxs.Any();
        public async Task FloodAsync()
        {
            int minimaCount = myMinimasIdxs.Count();

            using (var semaphore = new FieldsSemaphore(LevelOfParallelism == 1 ? 0 : LOGGED.Memory.Length))
            {
                // Create seeds
                var resultsSeeds = await myRunner.PerformAsync(minimaCount, LevelOfParallelism,
                                        (start, length) =>
                                        {
                                            var listSeeds = new List<IMST>(length);

                                            foreach (var minIdx in myMinimasIdxs.Skip(start).Take(length))
                                                listSeeds.Add(new PrimsMST(minIdx, LOGGED.Memory, LOGGED.Width, LevelOfParallelism == 1 ? (null as Func<int, bool>) : semaphore.TryTake));

                                            return listSeeds;
                                        }).ConfigureAwait(false);

                // Flood
                await myRunner.RunAsync(resultsSeeds,
                                        listSeeds => myProcessingFunctions.Flood(listSeeds))
                                        .ConfigureAwait(false);

                var segments = new List<IMST>(minimaCount);
                foreach (var lst in resultsSeeds)
                    segments.AddRange(lst);

                mySegments = segments;
            }
        }

        public async Task CreateSampleLayersAsync(SampleType smapleType, string prefix)
        {
            int segmentsCount = mySegments.Count();

            foreach (var sourceLayer in myCutedLayers.Values)
            {
                var targetMemory = new Memory<byte>(new byte[sourceLayer.Memory.Length]);

                await myRunner.RunAsync(segmentsCount, LevelOfParallelism,
                                        (start, length) => myProcessingFunctions.CreateSampleLayer(mySegments.Skip(start).Take(length), sourceLayer.Memory, targetMemory, smapleType)
                                        ).ConfigureAwait(false);


                var resultLayerID = $"{(string.IsNullOrWhiteSpace(prefix) ? nameof(myProcessingFunctions.CreateSampleLayer) : prefix)}_{sourceLayer.ID}";
                mySampleLayers[resultLayerID] = new RasterLayer(resultLayerID, targetMemory, sourceLayer.Width, sourceLayer.Height);
            }
        }

    }
}