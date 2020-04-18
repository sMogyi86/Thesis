using MARGO.BL.Graph;
using MARGO.BL.Img;
using MARGO.BL.Segment;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MARGO.BL
{
    public class Project
    {
        #region Services
        private readonly IIOService myIOService = MyServices.GetIO();
        private readonly IProcessingFunctions myProcessingFunctions = MyServices.GetProcessingFunctions();
        #endregion

        private readonly Runner myRunner = new Runner();
        public byte LevelOfParallelism { get; set; } = (byte)Environment.ProcessorCount;

        public static Project Instance { get; } = new Project();

        private IDictionary<string, RasterLayer> myOriginalLayers = new Dictionary<string, RasterLayer>();
        private IDictionary<string, RasterLayer> myCutedLayers = new ConcurrentDictionary<string, RasterLayer>();
        private IDictionary<string, RasterLayer> mySampleLayers = new Dictionary<string, RasterLayer>();
        public IEnumerable<RasterLayer> Layers => myOriginalLayers.Values
                                            .Concat(myCutedLayers.Values)
                                            .Concat(mySampleLayers.Values);
        public Variants<int> RAW { get; private set; }
        public Variants<byte> BYTES { get; private set; }
        public Variants<byte> LOGGED { get; private set; }
        public ReadOnlyMemory<byte> MINIMAS { get; private set; }
        private IEnumerable<int> myMinimasIdxs = Enumerable.Empty<int>();

        private IEnumerable<IMST> mySegments = Enumerable.Empty<IMST>();
        public ReadOnlyMemory<byte> CLASSIFIEDIMAGE { get; private set; }
        public IReadOnlyDictionary<byte, int> ColorMapping { get; private set; }


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

        public async Task Cut(int topLeftX, int topLeftY, int bottomRightX, int bottomRightY, string prefix)
        {
            await myRunner.ScheduleAsync(myOriginalLayers.Values, LevelOfParallelism,
                layer =>
                {
                    var clID = $"{(string.IsNullOrWhiteSpace(prefix) ? nameof(myProcessingFunctions.Cut) : prefix)}_{layer.ID}";
                    myCutedLayers[clID] = myProcessingFunctions.Cut(layer, topLeftX, topLeftY, bottomRightX, bottomRightY, clID);
                }).ConfigureAwait(false);
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
                }).ConfigureAwait(false);

            await Task.Run(() =>
            {
                myProcessingFunctions.PopulateStats(RAW);

                myProcessingFunctions.ReclassToByte(RAW, BYTES = new Variants<byte>(RAW.Width, RAW.Height));
                myProcessingFunctions.PopulateStats(BYTES);

                myProcessingFunctions.ReclassToByteLog(RAW, LOGGED = new Variants<byte>(RAW.Width, RAW.Height));
                myProcessingFunctions.PopulateStats(LOGGED);
            }).ConfigureAwait(false);
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

            minimaIds.Sort();
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

            bool isParallel = LevelOfParallelism > 1;

            if (isParallel)
                FieldsSemaphore.Initialize(LOGGED.Memory.Length);
            else
                FieldsSemaphore.Initialize(0);

            PrimsMST.Initalize(LOGGED.Width, isParallel);

            // Create seeds
            var resultsSeeds = await myRunner.PerformAsync(minimaCount, LevelOfParallelism,
                                    (start, length) =>
                                    {
                                        var listSeeds = new List<IMST>(length);

                                        foreach (var minIdx in myMinimasIdxs.Skip(start).Take(length))
                                            listSeeds.Add(new PrimsMST(minIdx, LOGGED.Memory));

                                        return listSeeds;
                                    }).ConfigureAwait(false);

            // Flood
            await myRunner.ScheduleAsync(resultsSeeds,
                                    LevelOfParallelism,
                                    listSeeds => myProcessingFunctions.Flood(listSeeds))
                                    .ConfigureAwait(false);

            var segments = new List<IMST>(minimaCount);
            foreach (var lst in resultsSeeds)
                segments.AddRange(lst);

            mySegments = segments;
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

        public async Task ClasifyAsync(SampleType sType, IEnumerable<ISampleGroup> samples)
        {
            int shortRegisterCount;

            if (myCutedLayers.Count > (shortRegisterCount = System.Numerics.Vector<short>.Count))
                throw new NotSupportedException($"Layer count [{myCutedLayers.Count}] higher than [{shortRegisterCount}].");


            var segmentStats = new ISegmentStats[myCutedLayers.Count];
            int i = 0;
            foreach (var lyr in myCutedLayers)
                segmentStats[i++] = new SegmentStatsDecorator(lyr.Value.Memory);

            IClassifier classifier = new MinDistClassifier();
            var categorySmaples = await Task.Run(
                                    () => classifier.CreateCategorySmaples(sType, samples, mySegments.Select(mst => mst.Items), segmentStats))
                                        .ConfigureAwait(false);

            MinDistClassifier.Initialize(categorySmaples);
            var classifiedImage = new byte[myCutedLayers.First().Value.Memory.Length];
            var mappings = classifier.CreateMappings(categorySmaples.Keys);
            var categoryMapping = mappings.CategoryMapping;

            await myRunner.RunAsync(mySegments.Count(), LevelOfParallelism,
                (start, length) =>
                {
                    Span<byte> sampleVector = stackalloc byte[segmentStats.Length];
                    Span<short> segmentBuffer = stackalloc short[shortRegisterCount];
                    segmentBuffer.Fill(0);
                    foreach (var segment in mySegments.Skip(start).Take(length).Select(mst => mst.Items))
                    {
                        classifier.CreateSample(sType, segment, segmentStats, sampleVector);

                        // 8=>16 byte=>short conversion
                        for (int j = 0; j < sampleVector.Length; j++)
                            segmentBuffer[j] = sampleVector[j];

                        int category = classifier.Classify(segmentBuffer);

                        foreach (var idx in segment)
                            classifiedImage[idx] = categoryMapping[category];
                    }
                }).ConfigureAwait(false);

            CLASSIFIEDIMAGE = classifiedImage;
            ColorMapping = mappings.ColorMapping;
        }
    }
}