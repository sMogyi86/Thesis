using BitMiracle.LibTiff.Classic;
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

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Load();

            this.TestCut();

            this.TestComposite();

            this.TestGenerateVariantImage();
        }

        private void Load()
        {
            myR = iOService.Load(_b40Path);
            myG = iOService.Load(_b30Path);
            myB = iOService.Load(_b30Path);
        }

        private void TestComposite()
            => this.SetUserImage(new TiffParts(myR.Width, myR.Height, myR.Data, myG.Data, myB.Data));

        private void SetUserImage(TiffParts tiffParts)
        {
            var img = compositeFactory.CreateTiff(tiffParts);

            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = img;
            imageSource.EndInit();

            this.UserImage = imageSource;
        }

        private void TestCut(int topLeftX = 1800, int topLeftY = 1600, int bottomRightX = 6300, int bottomRightY = 5800)
        {
            myR = processingFunctions.Cut(myR, topLeftX, topLeftY, bottomRightX, bottomRightY);
            myG = processingFunctions.Cut(myG, topLeftX, topLeftY, bottomRightX, bottomRightY);
            myB = processingFunctions.Cut(myB, topLeftX, topLeftY, bottomRightX, bottomRightY);
        }

        private void TestGenerateVariantImage()
        {
            byte range = 3;
            int length = myR.Width * myR.Height;

            var offsetValues = testServices.CalculateOffsetsFor(myR.Width, range);

            var vars = new int[length];
            Memory<int> variants = new Memory<int>(vars);

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

            processingFunctions.CalculateVariants(myR.Data, variants, offsetValues);
            //processingFunctions.CalculateVariants(myG.Data, variants, offsetValues);
            //processingFunctions.CalculateVariants(myB.Data, variants, offsetValues);

            Memory<byte> bytes = new Memory<byte>(new byte[length]);
            double ratio = ((double)byte.MaxValue) / vars.Max();
            processingFunctions.ReclassToByte(variants, bytes, ratio);
            this.SetUserImage(new TiffParts(myR.Width, myR.Height, bytes));

            //double ratio = vars.Max() / 16_777_216.0; // 2ˇ(3*8)
            //processingFunctions.ReclassToRGB(variants, ratio);

            //Memory<byte> red = new Memory<byte>(new byte[length]);
            //Memory<byte> green = new Memory<byte>(new byte[length]);
            //Memory<byte> blue = new Memory<byte>(new byte[length]);
            //processingFunctions.SplitToRGB(variants, red, green, blue);

            //this.SetUserImage(new TiffParts(myR.Width, myR.Height, red, green, blue));
        }
    }
}