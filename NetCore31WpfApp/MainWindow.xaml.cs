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
        private readonly TestServices services = new TestServices();
        private readonly UIServices uIServices = new UIServices();

        public byte[] UserImage { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var imageName = uIServices.OpenTiff();

            this.UserImage = services.ReadBytesAsync(imageName);
        }

        
    }
}
