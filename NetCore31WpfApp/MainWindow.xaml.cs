using BitMiracle.LibTiff.Classic;
using Microsoft.Win32;
using System;
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
        public byte[] UserImage { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd =
                new OpenFileDialog()
                {
                    InitialDirectory = @"c:\Users\z0040rwz\Documents\Private\OE NIK\_SzD\DATA\LE07_L1TP_188027_20011220_20170201_01_T2\",
                    Filter= @"GeoTIFF files(*.tif)|*.tif",
                    Multiselect = true,                    
                };

            if (ofd.ShowDialog().Value)
            {
                var tiff = Tiff.Open(ofd.FileNames.First(), "r");

                    

                this.UserImage = this.GetImageAsByteArray(ofd.FileNames.First());
            }
        }

        private byte[] GetImageAsByteArray(string imageName)
        {
            byte[] image = { };

            using (Stream stream = new FileStream(imageName, FileMode.Open))
            {
                var length = stream.Length;
                image = new byte[length];
                stream.Read(image, 0, (int)length);
            }

            return image;
        }
    }
}
