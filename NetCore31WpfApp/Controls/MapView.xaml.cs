using MARGO.Common;
using System.Windows.Controls;
using System.Windows.Input;

namespace MARGO.Controls
{
    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapView : UserControl
    {
        public MapView()
        {
            InitializeComponent();

            //myTimer.Elapsed += MyTimer_Elapsed;
        }

        //private void MyTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    myTimer.Stop();

        //    if (this.DataContext is IUIHelper helper)
        //        helper.TimerElapsedAt(myZoomAndPanControl.MousePosition);
        //}

        private void myZoomAndPanControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.DataContext is IUIHelper helper)
                helper.Handle?.Invoke(myZoomAndPanControl.MousePosition);
        }

        //private readonly Timer myTimer = new Timer(500);

        private void myZoomAndPanControl_MouseMove(object sender, MouseEventArgs e)
        {
            //myTimer.Stop();
            //myTimer.Start();
        }
    }
}
