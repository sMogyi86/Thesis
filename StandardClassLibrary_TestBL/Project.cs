using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace StandardClassLibraryTestBL
{
    public class Project
    {
        #region Services
        private readonly IIOService myIOService = Services.GetIO();
        private readonly IProcessingFunctions myProcessingFunctions = Services.GetProcessingFunctions();
        #endregion



        private readonly Dictionary<string, RasterLayer> myLayers;
        public IReadOnlyDictionary<string, RasterLayer> Layers => myLayers;
        public Variants<int> RAW { get; private set; }
        public Variants<byte> BYTES { get; private set; }
        public Variants<byte> LOGGED { get; private set; }



        public Project(IEnumerable<string> ids)
        {
            if (ids is null || !ids.Any())
                throw new ArgumentNullException(nameof(ids));

            myLayers = new Dictionary<string, RasterLayer>(ids.Count());
            foreach (var id in ids)
                myLayers[id] = null;
        }



        public void Load()
        {
            foreach (var id in myLayers.Keys)
                myLayers[id] = myIOService.Load(id);
        }

        public void Cut(int topLeftX = 1800, int topLeftY = 1600, int bottomRightX = 6300, int bottomRightY = 5800)
        {
            foreach (var kvp in myLayers)
                myLayers[kvp.Key] = myProcessingFunctions.Cut(kvp.Value, topLeftX, topLeftY, bottomRightX, bottomRightY);
        }

        public void CalculateVariantsWithStats(byte range = 3)
        {
            IProcessingFunctions processingFunctions = Services.GetProcessingFunctions();

            var firstLayer = myLayers.First().Value;
            int length = firstLayer.Width * firstLayer.Height;
            RAW = new Variants<int>(length);

            var offsetValues = Offsets.CalculateOffsetsFor(firstLayer.Width, range);

            foreach (var layer in myLayers.Values)
                myProcessingFunctions.CalculateVariants(layer.Memory, RAW.Data, offsetValues);

            processingFunctions.PopulateStats(RAW);
        }

        public void ReclassToByte()
        {
            BYTES = new Variants<byte>(RAW.Data.Length);
            myProcessingFunctions.ReclassToByte(RAW, BYTES);
        }

        public void ReclassToByteLog()
        {
            LOGGED = new Variants<byte>(RAW.Data.Length);
            myProcessingFunctions.ReclassToByte(RAW, LOGGED);
        }
    }
}
