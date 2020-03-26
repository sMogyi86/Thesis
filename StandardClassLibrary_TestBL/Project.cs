using MARGO.BL.Graph;
using MARGO.BL.Img;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MARGO.BL
{
    // TODO add cancellation option to async methods
    public class Project
    {
        #region Services
        private readonly IIOService myIOService = Services.GetIO();
        private readonly IImageFunctions myProcessingFunctions = Services.GetProcessingFunctions();
        private readonly GraphFuctions myGraphFuctions = new GraphFuctions();
        #endregion

        private readonly Runner myRunner = new Runner();

        public static Project Instance { get; } = new Project();

        private Dictionary<string, RasterLayer> myOriginalLayers = new Dictionary<string, RasterLayer>();
        private Dictionary<string, RasterLayer> myCutedLayers = new Dictionary<string, RasterLayer>();
        public IEnumerable<RasterLayer> Layers => myOriginalLayers.Values.Concat(myCutedLayers.Values);
        public Variants<int> RAW { get; private set; } // TODO eliminate visibility level
        public Variants<byte> BYTES { get; private set; } // TODO eliminate at all
        public Variants<byte> LOGGED { get; private set; }
        public ReadOnlyMemory<byte> MINIMAS => myMinimas.Minimas;
        private (ReadOnlyMemory<byte> Minimas, int Count) myMinimas;



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

        public void Cut(int topLeftX = 1800, int topLeftY = 1600, int bottomRightX = 6300, int bottomRightY = 5800)
        {
            foreach (var layer in myOriginalLayers.Values)
            {
                var cl = myProcessingFunctions.Cut(layer, topLeftX, topLeftY, bottomRightX, bottomRightY);
                myCutedLayers[cl.ID] = cl;
            }

        }

        public void CalculateVariantsWithStats(byte range = 3)
        {
            var firstLayer = myCutedLayers.First().Value;
            RAW = new Variants<int>(firstLayer.Width, firstLayer.Height);

            var offsetValues = Offsets.CalculateOffsetsFor(firstLayer.Width, range);

            foreach (var layer in myCutedLayers.Values)
                myProcessingFunctions.CalculateVariants(layer.Memory, RAW.Data, offsetValues);

            myProcessingFunctions.PopulateStats(RAW);
        }

        public async Task CalculateVariantsWithStatsAsync(byte range = 3)
        {
            var firstLayer = myCutedLayers.First().Value;
            RAW = new Variants<int>(firstLayer.Width, firstLayer.Height);
            var offsetValues = Offsets.CalculateOffsetsFor(firstLayer.Width, range);

            int pc = Environment.ProcessorCount;

            await myRunner.RunAsync(myCutedLayers.First().Value.Memory.Length, pc,
                (start, length) =>
                {
                    foreach (var layer in myCutedLayers.Values)
                        myProcessingFunctions.CalculateVariants(layer.Memory, RAW.Data, offsetValues, start, length);
                })
                .ContinueWith((t) => myProcessingFunctions.PopulateStats(RAW), CancellationToken.None, TaskContinuationOptions.RunContinuationsAsynchronously, TaskScheduler.Default)
                .ConfigureAwait(false); ;
        }

        public void ReclassToByte() => myProcessingFunctions.ReclassToByte(RAW, BYTES = new Variants<byte>(RAW.Width, RAW.Height));

        public void ReclassToByteLog() => myProcessingFunctions.ReclassToByteLog(RAW, LOGGED = new Variants<byte>(RAW.Width, RAW.Height));

        public async Task Save(Image image, string id) => await myIOService.Save(image, id).ConfigureAwait(false);

        public async Task FindMinimasAsync()
        {
            byte range = 3;
            var minimas = new byte[LOGGED.Memory.Length];

            var offsetValues = Offsets.CalculateOffsetsFor(LOGGED.Width, range);

            int pc = Environment.ProcessorCount;

            await myRunner.RunAsync(LOGGED.Memory.Length, pc, (start, length) =>
            {
                foreach (var layer in myCutedLayers.Values)
                    myProcessingFunctions.FindMinimas(LOGGED.Memory, minimas, offsetValues, start, length);
            }).ConfigureAwait(false);

            myMinimas = (Minimas: minimas, Count: minimas.Count(v => v == byte.MaxValue));
        }

        public async Task FloodAsync()
        {
            int pc = 1;// Environment.ProcessorCount;
            var nodes = new Memory<Node>(new Node[LOGGED.Memory.Length]);
            var seeds = new List<IMST>(myMinimas.Count);

            await myRunner.RunAsync(LOGGED.Memory.Length, pc,
                (start, length) =>
                {
                    myGraphFuctions.CreateNodes(LOGGED.Memory, nodes, start, length);
                }).ConfigureAwait(false);

            await myRunner.RunAsync(nodes.Length, pc,
                (start, length) =>
                {
                    myGraphFuctions.InitializeEdges(LOGGED.Width, nodes, start, length);
                }).ConfigureAwait(false);

            using (var semaphore = new FieldsSemaphore(nodes.Length))
            {
                var sdsLst = await myRunner.PerformAsync(myMinimas.Count, pc,
                                (start, length) =>
                                {
                                    var sds = new List<IMST>();
                                    myGraphFuctions.CreateSeeds(semaphore.TryTake, nodes, myMinimas.Minimas, sds, start, length);
                                    return sds;
                                }).ConfigureAwait(false);

                foreach (var lst in sdsLst)
                    seeds.AddRange(lst);

                await myRunner.RunAsync(nodes.Length, pc,
                    (start, length) =>
                    {
                        myGraphFuctions.Flood(seeds.Skip(start).Take(length));
                    }).ConfigureAwait(false);
            }
        }

    }
}