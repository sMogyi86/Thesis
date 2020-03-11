using MARGO.BL;
using MARGO.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MARGO
{
    class MapVM : INotifyPropertyChanged
    {
        #region Services
        private readonly IImageFactory myImageFactory = Services.GetImageFactory();
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly MsgBox myUIServices = new MsgBox();

        public Project Project { get; private set; }

        public ICommand OpenCommand => new DelegateCommand(() =>
        {
            Project = new Project(myUIServices.GetLayerFiles());
        });

        public ICommand LoadCommand => Project is null ? null : new DelegateCommand(
            Project.Load,
            () => Project != null);

        public IEnumerable<RasterLayer> Chanels => Project?.Layers.Values;
        public RasterLayer Red { get; set; }
        public RasterLayer Green { get; set; }
        public RasterLayer Blue { get; set; }

        public ICommand ComposeCommand => new DelegateCommand(
            () =>
            {
                Maps.Add(myImageFactory.CreateImage($"{Red.ID}_{Green.ID}_{Blue.ID}", new ImageParts(Red.Width, Red.Height, Red.Memory, Green.Memory, Blue.Memory)));
                CurrentMap = Maps.Last();
            },
            () => Red != null && Green != null && Blue != null);

        public ICommand TopLeftCommand => null; // new DelegateCommand(null, () => false);

        public Point TopLeftPoint { get; set; } = new Point(1800, 1600);

        public ICommand BottomRightCommand => null; // new DelegateCommand(null, () => false);

        public Point BottomRightPoint { get; set; } = new Point(6300, 5800);

        public ICommand CutCommand => Project is null ? null : new DelegateCommand(
            () => Project.Cut(TopLeftPoint.X, TopLeftPoint.Y, BottomRightPoint.X, BottomRightPoint.Y),
            () => Project != null && TopLeftPoint != Point.Empty && BottomRightPoint != Point.Empty);

        public byte Range { get; set; } = 3;
        public ICommand VariantsCommand => Project is null ? null : new DelegateCommand<byte>(
            Project.CalculateVariantsWithStats,
            range => Project != null && range > 1 && range % 2 == 1);

        public ObservableCollection<Image> Maps { get; } = new ObservableCollection<Image>();

        private Image currentMap;
        public Image CurrentMap
        {
            get => currentMap;
            set
            {
                currentMap = value;

                if (currentMap != null)
                {
                    var imageSource = new BitmapImage();
                    imageSource.BeginInit();
                    imageSource.StreamSource = currentMap.Stream;
                    imageSource.EndInit();

                    this.ImageSource = imageSource;
                }
            }
        }
        public ImageSource ImageSource { get; private set; }

        public ICommand SaveMapCommand => Project is null ? null : new DelegateCommand<Image>(
            async image =>
            {
                var savePath = myUIServices.GetSavePath();

                if (!string.IsNullOrWhiteSpace(savePath))
                {
                    await Project.Save(image, savePath);

                    myUIServices.ShowInfo("Map saved.");
                }
            },
            img => img != null);
    }
}
