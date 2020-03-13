using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MARGO.BL
{
    public class Project
    {
        #region Services
        private readonly IIOService myIOService = Services.GetIO();
        private readonly IProcessingFunctions myProcessingFunctions = Services.GetProcessingFunctions();
        #endregion

        public static Project Instance { get; } = new Project();

        private Dictionary<string, RasterLayer> myOriginalLayers = new Dictionary<string, RasterLayer>();
        private Dictionary<string, RasterLayer> myCutedLayers = new Dictionary<string, RasterLayer>();
        public IEnumerable<RasterLayer> Layers => myOriginalLayers.Values.Concat(myCutedLayers.Values);
        public Variants<int> RAW { get; private set; }
        public Variants<byte> BYTES { get; private set; }
        public Variants<byte> LOGGED { get; private set; }


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

            var slicer = new Slicer();
            int pc = Environment.ProcessorCount;
            var lengths = slicer.Slice(myCutedLayers.First().Value.Memory.Length, pc);
            var tasks = new Task[pc];

            int start = 0;
            int i = 0;
            foreach (var l in lengths)
            {
                int sectionStart = start;

                tasks[i++] = Task.Run(() =>
                {
                    foreach (var layer in myCutedLayers.Values)
                        myProcessingFunctions.CalculateVariants(layer.Memory, RAW.Data, offsetValues, sectionStart, l);
                });

                start += l;
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            myProcessingFunctions.PopulateStats(RAW);
        }

        public void ReclassToByte()
            => myProcessingFunctions.ReclassToByte(RAW, BYTES = new Variants<byte>(RAW.Width, RAW.Height));

        public void ReclassToByteLog()
            => myProcessingFunctions.ReclassToByteLog(RAW, LOGGED = new Variants<byte>(RAW.Width, RAW.Height));

        public async Task Save(Image image, string id)
            => await myIOService.Save(image, id).ConfigureAwait(false);
    }
}
