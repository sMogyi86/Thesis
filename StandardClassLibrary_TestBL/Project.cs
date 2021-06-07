using MARGO.BL.Graph;
using MARGO.BL.Img;
using MARGO.BL.Segment;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
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
        public byte LevelOfParallelism { get; set; }

        public static Project Instance { get; } = new Project();

        private IDictionary<string, RasterLayer> myOriginalLayers = new Dictionary<string, RasterLayer>();
        private IDictionary<string, RasterLayer> myCutedLayers = new ConcurrentDictionary<string, RasterLayer>();
        private IDictionary<string, RasterLayer> mySampleLayers = new Dictionary<string, RasterLayer>();
        private string TimeStamp => DateTime.Now.ToString("hhmmss", CultureInfo.InvariantCulture);
        public IEnumerable<RasterLayer> Layers => myOriginalLayers.Values
                                            .Concat(myCutedLayers.Values)
                                            .Concat(mySampleLayers.Values);
        public Variants<int> RAW { get; private set; }
        public Variants<byte> BYTES { get; private set; }
        public Variants<byte> LOGGED { get; private set; }
        public ReadOnlyMemory<byte> MINIMAS { get; private set; }
        public int MinimasCount { get; private set; }
        private IEnumerable<int> myMinimasIdxs = Enumerable.Empty<int>();

        private IEnumerable<IMST> mySegments = Enumerable.Empty<IMST>();
        public ReadOnlyMemory<byte> CLASSIFIEDIMAGE { get; private set; }
        public IReadOnlyDictionary<byte, uint> ColorMapping { get; private set; }


        private Project()
        {
            int processorCount = Environment.ProcessorCount;
            processorCount -= 2;
            LevelOfParallelism = (byte)(processorCount > 0 ? processorCount : 1);
        }



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

        public async Task Cut(int topLeftX, int topLeftY, int bottomRightX, int bottomRightY, string prefix, CancellationToken token)
        {
            string now = TimeStamp;

            await myRunner.ScheduleAsync(myOriginalLayers.Values, LevelOfParallelism,
                layer =>
                {
                    var clID = $"{now}_{(string.IsNullOrWhiteSpace(prefix) ? nameof(myProcessingFunctions.Cut) : prefix)}_{layer.ID}";
                    myCutedLayers[clID] = myProcessingFunctions.Cut(layer, topLeftX, topLeftY, bottomRightX, bottomRightY, clID);
                }, token).ConfigureAwait(false);
        }

        public bool CanCalcVariants => myCutedLayers.Any();
        public async Task CalculateVariantsWithStatsAsync(byte range, CancellationToken token)
        {
            var firstLayer = myCutedLayers.First().Value;
            RAW = new Variants<int>(firstLayer.Width, firstLayer.Height);
            var offsetValues = Offsets.CalculateOffsetsFor(firstLayer.Width, range);

            await myRunner.RunAsync(myCutedLayers.First().Value.Memory.Length, LevelOfParallelism,
                (start, length) =>
                {
                    foreach (var layer in myCutedLayers.Values)
                    {
                        myProcessingFunctions.CalculateVariants(layer.Memory, RAW.Data, offsetValues, start, length);
                        token.ThrowIfCancellationRequested();
                    }
                }, token).ConfigureAwait(false);

            if (LevelOfParallelism == 1)
            {
                await Task.Run(() =>
                {
                    myProcessingFunctions.PopulateStats(RAW);

                    token.ThrowIfCancellationRequested();
                    myProcessingFunctions.ReclassToByte(RAW, BYTES = new Variants<byte>(RAW.Width, RAW.Height));
                    myProcessingFunctions.PopulateStats(BYTES);

                    token.ThrowIfCancellationRequested();
                    myProcessingFunctions.ReclassToByteLog(RAW, LOGGED = new Variants<byte>(RAW.Width, RAW.Height));
                    myProcessingFunctions.PopulateStats(LOGGED);
                }, token).ConfigureAwait(false);
            }
            else
            {
                await Task.Run(() =>
                {
                    myProcessingFunctions.PopulateStats(RAW);
                }, token).ConfigureAwait(false);

                await Task.WhenAll(new Task[2]
                {
                    Task.Run(()=>
                    {
                        myProcessingFunctions.ReclassToByte(RAW, BYTES = new Variants<byte>(RAW.Width, RAW.Height));
                        myProcessingFunctions.PopulateStats(BYTES);
                    }, token),
                    Task.Run(()=>
                    {
                        myProcessingFunctions.ReclassToByteLog(RAW, LOGGED = new Variants<byte>(RAW.Width, RAW.Height));
                        myProcessingFunctions.PopulateStats(LOGGED);
                    }, token)
                }).ConfigureAwait(false);
            }
        }

        public async Task FindMinimasAsync(byte range, CancellationToken token)
        {
            var offsetValues = Offsets.CalculateOffsetsFor(LOGGED.Width, range);

            var resultMinimas = await myRunner.PerformAsync(LOGGED.Memory.Length, LevelOfParallelism,
                                    (start, length) =>
                                    {
                                        var listMins = new List<int>();
                                        myProcessingFunctions.FindMinimas(LOGGED.Memory, listMins, offsetValues, start, length);
                                        return listMins;
                                    }, token).ConfigureAwait(false);

            token.ThrowIfCancellationRequested();
            var minimaIds = new List<int>(resultMinimas.Select(lst => lst.Count).Sum());
            foreach (var listMins in resultMinimas)
                minimaIds.AddRange(listMins);

            minimaIds.Sort();
            myMinimasIdxs = minimaIds;

            var minimas = new byte[LOGGED.Memory.Length];
            foreach (var idx in myMinimasIdxs)
                minimas[idx] = byte.MaxValue;

            MINIMAS = minimas;
            MinimasCount = minimaIds.Count;
        }

        public bool CanFlood => myMinimasIdxs.Any();
        public async Task FloodAsync(bool isV1, CancellationToken token)
        {
            PrimsMST.Initalize(LOGGED.Width);

            IEnumerable<IMST> segments;
            if (isV1)
            {
                FieldsSemaphore.Initialize(LOGGED.Memory.Length, true);
                segments = await this.FloodAsyncFirst(token).ConfigureAwait(false);
            }
            else
            {
                FieldsSemaphore.Initialize(LOGGED.Memory.Length, false);
                segments = await this.FloodAsyncSecond(token).ConfigureAwait(false);
            }

            mySegments = segments;
        }

        private async Task<IEnumerable<IMST>> FloodAsyncFirst(CancellationToken token)
        {
            int minimaCount = myMinimasIdxs.Count();

            // Create seeds
            var seeds = await myRunner.PerformAsync(minimaCount, LevelOfParallelism,
                                    (start, length) =>
                                    {
                                        var listSeeds = new List<IMST>(length);

                                        foreach (var minIdx in myMinimasIdxs.Skip(start).Take(length))
                                            listSeeds.Add(new PrimsMST(minIdx, LOGGED.Memory));

                                        return listSeeds;
                                    }, token).ConfigureAwait(false);

            token.ThrowIfCancellationRequested();

            // Flood
            await myRunner.ScheduleAsync(seeds,
                                    LevelOfParallelism,
                                    seedsPatrLst =>
                                    {
                                        do
                                        {
                                            foreach (var mst in seedsPatrLst.Where(s => !s.Terminated))
                                                mst.DoStep();

                                            token.ThrowIfCancellationRequested();
                                        } while (seedsPatrLst.Any(s => !s.Terminated));
                                    }, token)
                                    .ConfigureAwait(false);

            token.ThrowIfCancellationRequested();
            var segments = new List<IMST>(minimaCount);
            foreach (var lst in seeds)
                segments.AddRange(lst);

            return segments;
        }

        private async Task<IEnumerable<IMST>> FloodAsyncSecond(CancellationToken token) => await Task.Run(() =>
        {
            var segments = new List<PrimsMST>(myMinimasIdxs.Select(m => new PrimsMST(m, LOGGED.Memory)));

            LinkedList<PrimsMST> seeds = new LinkedList<PrimsMST>(segments);
            while (seeds.Any())
            {
                token.ThrowIfCancellationRequested();

                #region Clean from terminated, and find lowest
                byte lowestMinima = byte.MaxValue;
                LinkedListNode<PrimsMST> current = seeds.First;
                LinkedListNode<PrimsMST> next;
                while (current.Next != null)
                {
                    next = current.Next;

                    if (current.Value.Terminated)
                        seeds.Remove(current);
                    else if (current.Value.CurrentLevel < lowestMinima)
                        lowestMinima = current.Value.CurrentLevel;

                    current = next;
                }

                if (current.Value.Terminated)
                    seeds.Remove(current);
                else if (current.Value.CurrentLevel < lowestMinima)
                    lowestMinima = current.Value.CurrentLevel;
                #endregion

                token.ThrowIfCancellationRequested();

                if (!seeds.Any())
                    break;

                // Step only with the the lowest ones
                foreach (var seed in seeds)
                    if (seed.CurrentLevel == lowestMinima)
                        seed.DoStep();
            }

            return segments;
        }, token).ConfigureAwait(false);


        public async Task CreateSampleLayersAsync(SampleType smapleType, string prefix, CancellationToken token)
        {
            string now = TimeStamp;
            int segmentsCount = mySegments.Count();

            foreach (var sourceLayer in myCutedLayers.Values)
            {
                var targetMemory = new Memory<byte>(new byte[sourceLayer.Memory.Length]);

                await myRunner.RunAsync(segmentsCount, LevelOfParallelism,
                                        (start, length) => myProcessingFunctions.CreateSampleLayer(mySegments.Skip(start).Take(length), sourceLayer.Memory, targetMemory, smapleType)
                                        , token).ConfigureAwait(false);


                var resultLayerID = $"{now}_{(string.IsNullOrWhiteSpace(prefix) ? nameof(myProcessingFunctions.CreateSampleLayer) : prefix)}_{sourceLayer.ID}";
                mySampleLayers[resultLayerID] = new RasterLayer(resultLayerID, targetMemory, sourceLayer.Width, sourceLayer.Height);
            }
        }

        public async Task ClassifyAsync(SampleType sType, IEnumerable<ISampleGroup> samples, CancellationToken token)
        {
            int shortRegisterCount;

            if (myCutedLayers.Count > (shortRegisterCount = System.Numerics.Vector<short>.Count))
                throw new NotSupportedException($"Layer count [{myCutedLayers.Count}] higher than [{shortRegisterCount}].");


            var segmentStatsEXT = new ISegmentStats[myCutedLayers.Count];
            int i = 0;
            foreach (var lyr in myCutedLayers)
                segmentStatsEXT[i++] = new SegmentStats(lyr.Value.Memory);

            IClassifier classifier = new MinDistClassifier();
            var categorySmaples = classifier.CreateCategorySamples(sType, samples, mySegments.Select(mst => mst.Items), segmentStatsEXT);

            MinDistClassifier.Initialize(categorySmaples);
            var classifiedImage = new byte[myCutedLayers.First().Value.Memory.Length];
            var mappings = classifier.CreateMappings(categorySmaples.Keys);
            var categoryMapping = mappings.CategoryMapping;

            await myRunner.RunAsync(mySegments.Count(), LevelOfParallelism,
                (start, length) =>
                {
                    var segmentStatsINT = new ISegmentStats[myCutedLayers.Count];
                    int i = 0;
                    foreach (var lyr in myCutedLayers)
                        segmentStatsINT[i++] = new SegmentStats(lyr.Value.Memory);

                    Span<byte> sampleVector = stackalloc byte[segmentStatsINT.Length];
                    Span<short> segmentBuffer = stackalloc short[shortRegisterCount];
                    segmentBuffer.Fill(0);
                    int t = 0;
                    foreach (var segment in mySegments.Skip(start).Take(length).Select(mst => mst.Items))
                    {
                        if (t-- < 0)
                        {
                            token.ThrowIfCancellationRequested();
                            t = 20;
                        }

                        classifier.CreateSample(sType, segment, segmentStatsINT, sampleVector);

                        // 8=>16 byte=>short conversion
                        for (int j = 0; j < sampleVector.Length; j++)
                            segmentBuffer[j] = sampleVector[j];

                        uint category = classifier.Classify(segmentBuffer);

                        foreach (var idx in segment)
                            classifiedImage[idx] = categoryMapping[category];
                    }
                }, token).ConfigureAwait(false);

            CLASSIFIEDIMAGE = classifiedImage;
            ColorMapping = mappings.ColorMapping;
        }
    }
}