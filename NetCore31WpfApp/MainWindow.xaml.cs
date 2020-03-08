using BitMiracle.LibTiff.Classic;
using LiveCharts;
using LiveCharts.Configurations;
using LiveCharts.Wpf;
using Microsoft.Win32;
using StandardClassLibraryTestBL;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetCore31WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly static string _b50Path = @"D:\Segment\L5188027_02720060719_B50.TIF";
        private readonly static string _b40Path = @"D:\Segment\L5188027_02720060719_B40.TIF";
        private readonly static string _b30Path = @"D:\Segment\L5188027_02720060719_B30.TIF";
        private readonly static string _b20Path = @"D:\Segment\L5188027_02720060719_B20.TIF";
        private readonly static string _b10Path = @"D:\Segment\L5188027_02720060719_B10.TIF";



        private readonly UIServices uIServices = new UIServices();
        private readonly TestServices testServices = new TestServices();
        private readonly IIOService iOService = Services.GetIO();
        private readonly ICompositeFactory compositeFactory = Services.GetCompositeFactory();
        private readonly IProcessingFunctions processingFunctions = Services.GetProcessingFunctions();

        private IRasterLayer myR;
        private IRasterLayer myG;
        private IRasterLayer myB;

        public object UserImage { get; private set; }
        public SeriesCollection SeriesCollection { get; set; }
        public int Take { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Charting.For<int[]>(Mappers.Xy<int[]>().X(buci => buci[0]).Y(buci => buci[1]));
            Charting.For<double[]>(Mappers.Xy<double[]>().X(buci => buci[0]).Y(buci => buci[1]));
            Charting.For<Reclassed>(Mappers.Xy<Reclassed>().X(r => r.value).Y(r => r.count));

            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Start(object sender, RoutedEventArgs e)
        {
            this.Load();

            this.TestCut();

            this.SetUserImage(new TiffParts(myR.Width, myR.Height, myR.Data, myG.Data, myB.Data));

            this.TestGenerateVariantValues();

            this.TestCreateCahces();

            this.TestReclassToByte();
        }

        private Dictionary<int, int> varsDict;
        private IEnumerable<int[]> varsValues;
        private int[] variants;
        private Memory<int> varsMemory;

        private void Load()
        {
            myR = iOService.Load(_b40Path);
            myG = iOService.Load(_b30Path);
            myB = iOService.Load(_b20Path);
        }


        private void TestCut(int topLeftX = 1800, int topLeftY = 1600, int bottomRightX = 6300, int bottomRightY = 5800)
        {
            myR = processingFunctions.Cut(myR, topLeftX, topLeftY, bottomRightX, bottomRightY);
            myG = processingFunctions.Cut(myG, topLeftX, topLeftY, bottomRightX, bottomRightY);
            myB = processingFunctions.Cut(myB, topLeftX, topLeftY, bottomRightX, bottomRightY);
        }

        private void TestGenerateVariantValues()
        {
            byte range = 3;
            int length = myR.Width * myR.Height;

            var offsetValues = testServices.CalculateOffsetsFor(myR.Width, range);

            variants = new int[length];
            varsMemory = new Memory<int>(variants);

            //byte count = (byte)Environment.ProcessorCount;
            //var slices = new Slicer().Slice(myR.Height, count);
            //var tasks = new List<Task>(count);
            //foreach (var kvp in slices)
            //{
            //    for (int i = 0; i < kvp.Key; i++)
            //    {
            //        //tasks.Add();
            //    }
            //}

            processingFunctions.CalculateVariants(myR.Data, varsMemory, offsetValues);
            processingFunctions.CalculateVariants(myG.Data, variants, offsetValues);
            processingFunctions.CalculateVariants(myB.Data, variants, offsetValues);
        }

        private void TestCreateCahces()
        {
            var span = varsMemory.Span;
            varsDict = new Dictionary<int, int>();
            for (int i = 0; i < varsMemory.Length; i++)
            {
                if (varsDict.ContainsKey(span[i]))
                    varsDict[span[i]]++;
                else
                    varsDict[span[i]] = 1;
            }
            varsValues = varsDict.OrderBy(kvp => kvp.Key).Select(kvp => new int[2] { kvp.Key, kvp.Value }).ToList();
            Take = varsValues.Count();
        }

        private byte[] bytes;
        private Memory<byte> bytesMemory;
        private Dictionary<byte, int> bytesDict;
        private IEnumerable<Reclassed> reclasseds;
        private void TestReclassToByte()
        {
            bytes = new byte[varsMemory.Length];
            bytesMemory = new Memory<byte>(bytes);
            double b = Math.Pow(variants.Max(), 1.0 / byte.MaxValue);

            Dictionary<int, byte> mapping = new Dictionary<int, byte>(this.varsDict.Count);
            foreach (var integer in this.varsDict.Keys)
                mapping[integer] = (byte)Math.Log(integer, b);

            var span = bytesMemory.Span;
            for (int i = 0; i < variants.Length; i++)
            {
                span[i] = mapping[variants[i]];
            }




            bytesDict = new Dictionary<byte, int>(this.varsDict.Count);
            foreach (var integer in this.varsDict.Keys)
                bytesDict[mapping[integer]] = varsDict[integer];

            reclasseds = bytesDict.OrderBy(kvp => kvp.Key).Select(kvp => new Reclassed() { value = kvp.Key, count = kvp.Value }).ToList();


            //double ratio = vars.Max() / 16_777_216.0; // 2ˇ(3*8)
            //processingFunctions.ReclassToRGB(variants, ratio);

            //Memory<byte> red = new Memory<byte>(new byte[length]);
            //Memory<byte> green = new Memory<byte>(new byte[length]);
            //Memory<byte> blue = new Memory<byte>(new byte[length]);
            //processingFunctions.SplitToRGB(variants, red, green, blue);

            //this.SetUserImage(new TiffParts(myR.Width, myR.Height, red, green, blue));
        }

        private struct Reclassed
        {
            public byte value;
            public int count;
        }

        private void TestLiveCharts()
        {
            SeriesCollection = new SeriesCollection
            {
                new LineSeries()
                {
                    DataLabels = false,
                    Values = new ChartValues<int[]>(varsValues.Take(Take).ToList())
                }
            };
        }

        private void ShowVariants(object sender, RoutedEventArgs e)
        {
            this.TestLiveCharts();
        }

        private IEnumerable<double[]> doubles;
        private void ShowDoubles(object sender, RoutedEventArgs e)
        {
            int max = variants.Max();

            double ratio = ((double)byte.MaxValue) / max;

            doubles = varsValues.Select(b => new double[2] { ((byte)(ratio * b[0])), b[1] }).ToList();

            SeriesCollection = new SeriesCollection
            {
                new LineSeries()
                {
                    DataLabels = false,
                    Values = new ChartValues<double[]>(doubles.Take(Take).ToList())
                }
            };
        }


        private void ShowBytes(object sender, RoutedEventArgs e)
        {
            SeriesCollection = new SeriesCollection
            {
                new LineSeries()
                {
                    DataLabels = false,
                    Values = new ChartValues<Reclassed>(reclasseds.Take(Take).ToList())
                }
            };
        }

        private void SetUserImage(TiffParts tiffParts)
        {
            var img = compositeFactory.CreateTiff(tiffParts);

            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = img;
            imageSource.EndInit();

            this.UserImage = imageSource;
        }

        private void Orinial(object sender, RoutedEventArgs e)
        {
            this.SetUserImage(new TiffParts(myR.Width, myR.Height, myR.Data, myG.Data, myB.Data));
        }

        private void Variants(object sender, RoutedEventArgs e)
        {
            this.SetUserImage(new TiffParts(myR.Width, myR.Height, bytesMemory));
        }
    }
}