using BitMiracle.LibTiff.Classic;
using Microsoft.Win32;
using StandardClassLibraryTestBL;
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
        private readonly UIServices uIServices = new UIServices();
        private readonly IIOService iOService = new TiffIO();
        private readonly TestServices services = new TestServices();

        public object UserImage { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            RenderOptions.SetBitmapScalingMode(this.Image, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(this.Image, EdgeMode.Aliased);

            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var imageName = uIServices.OpenTiff();

            //this.UserImage = services.ReadBytesAsync(imageName);
        }

        private WriteableBitmap writeableBitmap;
        public void Testing()
        {
            var r1 = iOService.Read(@"");
            var r2 = iOService.Read(@"");
            var r3 = iOService.Read(@"");

            this.writeableBitmap = new WriteableBitmap(
                r1.With,
                r1.Height,
                96,
                96,
                PixelFormats.Bgr24,
                null);


        }

        void DrawPixel(IRaster r, IRaster g, IRaster b)
        {
            int column = (int)e.GetPosition(i).X;
            int row = (int)e.GetPosition(i).Y;

            try
            {
                // Reserve the back buffer for updates.
                writeableBitmap.Lock();

                unsafe
                {
                    // Get a pointer to the back buffer.
                    IntPtr pBackBuffer = writeableBitmap.BackBuffer;

                    // Find the address of the pixel to draw.
                    pBackBuffer += row * writeableBitmap.BackBufferStride;
                    pBackBuffer += column * 4;

                    // Compute the pixel's color.
                    int color_data = 255 << 16; // R
                    color_data |= 128 << 8;   // G
                    color_data |= 255 << 0;   // B

                    // Assign the color data to the pixel.
                    *((int*)pBackBuffer) = color_data;
                }

                // Specify the area of the bitmap that changed.
                writeableBitmap.AddDirtyRect(new Int32Rect(column, row, 1, 1));
            }
            finally
            {
                // Release the back buffer and make it available for display.
                writeableBitmap.Unlock();
            }
        }

    }
}
