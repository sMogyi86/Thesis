using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MARGO.BL
{
    // TODO add cancellation option to async methods
    public class Project
    {
        #region Services
        private readonly IIOService myIOService = Services.GetIO();
        private readonly IProcessingFunctions myProcessingFunctions = Services.GetProcessingFunctions();
        #endregion

        private readonly Runner myRunner = new Runner();

        public static Project Instance { get; } = new Project();

        private Dictionary<string, RasterLayer> myOriginalLayers = new Dictionary<string, RasterLayer>();
        private Dictionary<string, RasterLayer> myCutedLayers = new Dictionary<string, RasterLayer>();
        public IEnumerable<RasterLayer> Layers => myOriginalLayers.Values.Concat(myCutedLayers.Values);
        public Variants<int> RAW { get; private set; } // TODO eliminate visibility level
        public Variants<byte> BYTES { get; private set; }
        public Variants<byte> LOGGED { get; private set; }
        public ReadOnlyMemory<byte> MINIMAS { get; private set; }
        public SeedRepository SeedRepository { get; private set; }


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

        public void CalculateVariantsWithStats(byte range)
        {
            var firstLayer = myCutedLayers.First().Value;
            RAW = new Variants<int>(firstLayer.Width, firstLayer.Height);

            var offsetValues = Offsets.CalculateOffsetsFor(firstLayer.Width, range);

            foreach (var layer in myCutedLayers.Values)
                myProcessingFunctions.CalculateVariants(layer.Memory, RAW.Data, offsetValues);

            myProcessingFunctions.PopulateStats(RAW);
        }

        public async Task CalculateVariantsWithStatsAsync(byte range)
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
            var minimas = new int[LOGGED.Memory.Length];
            var minimasMem = new Memory<int>(minimas);

            var offsetValues = Offsets.CalculateOffsetsFor(LOGGED.Width, range);

            int pc = Environment.ProcessorCount;

            await myRunner.RunAsync(LOGGED.Memory.Length, pc, (start, length) =>
            {
                foreach (var layer in myCutedLayers.Values)
                    myProcessingFunctions.FindMinimas(LOGGED.Memory, minimasMem, offsetValues, start, length);
            }).ConfigureAwait(false);

            SeedRepository = new SeedRepository(minimasMem);

            var minImg = new byte[minimasMem.Length];
            for (int i = 0; i < minimas.Length; i++)
                minImg[i] = minimas[i] > 0 ? byte.MaxValue : byte.MinValue;

            MINIMAS = minImg;
        }
    }
}
