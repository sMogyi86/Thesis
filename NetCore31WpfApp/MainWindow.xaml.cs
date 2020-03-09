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
        private readonly ICompositeFactory compositeFactory = Services.GetCompositeFactory();
        
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
            //this.SetUserImage(new TiffParts(myR.Width, myR.Height, myR.Data, myG.Data, myB.Data));
        }

     
        private void TestLiveCharts()
        {
            //SeriesCollection = new SeriesCollection
            //{
            //    new LineSeries()
            //    {
            //        DataLabels = false,
            //        Values = new ChartValues<int[]>(varsValues.Take(Take).ToList())
            //    }
            //};
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

        private void Original(object sender, RoutedEventArgs e)
        {
            //this.SetUserImage(new TiffParts(myR.Width, myR.Height, myR.Data, myG.Data, myB.Data));
        }

        private void Variants(object sender, RoutedEventArgs e)
        {
            //this.SetUserImage(new TiffParts(myR.Width, myR.Height, bytesMemory));
        }


        private struct Reclassed
        {
            public byte value;
            public int count;
        }
    }
}