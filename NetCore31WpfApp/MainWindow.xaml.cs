﻿using BitMiracle.LibTiff.Classic;
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

        private IRaster myR;
        private IRaster myG;
        private IRaster myB;

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

            this.TestComposite();
        }

        private void Load()
        {
            myR = iOService.Load(_b40Path);
            myG = iOService.Load(_b30Path);
            myB = iOService.Load(_b30Path);
        }

        private void TestComposite()
        {
            var compositeParts = new TiffParts(myR.With, myR.Height, myR.Data, myG.Data, myB.Data);
            var img = compositeFactory.CreateTiff(compositeParts);

            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = img;
            imageSource.EndInit();

            this.UserImage = imageSource;
        }
    }
}