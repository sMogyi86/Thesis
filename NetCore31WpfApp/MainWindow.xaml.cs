using LiveCharts;
using LiveCharts.Configurations;
using System.Windows;

namespace MARGO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Charting.For<int[]>(Mappers.Xy<int[]>().X(buci => buci[0]).Y(buci => buci[1]));
            Charting.For<double[]>(Mappers.Xy<double[]>().X(buci => buci[0]).Y(buci => buci[1]));
            Charting.For<Reclassed>(Mappers.Xy<Reclassed>().X(r => r.value).Y(r => r.count));
        }

        private struct Reclassed //~5B*width*height
        {
            public byte value;
            public int count;
        }
    }
}