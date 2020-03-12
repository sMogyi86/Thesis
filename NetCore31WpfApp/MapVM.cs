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
    class MapVM : ObservableBase
    {
        #region Services
        private readonly IImageFactory myImageFactory = Services.GetImageFactory();
        #endregion

        private readonly MsgBox myUIServices = new MsgBox();

        private Project Project => Project.Instance;

        public ICommand LoadCommand => new DelegateCommand(
            () =>
            {
                var ids = myUIServices.GetLayerFiles();

                if (ids.Any())
                {
                    Project.Load(ids);
                    this.RaisePropertyChanged(nameof(Chanels));
                }
            });

        public IEnumerable<RasterLayer> Chanels => Project.Layers;
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

        //public ICommand TopLeftCommand =>  new DelegateCommand(null, () => false);
        public Point TopLeftPoint { get; set; } = new Point(1800, 1600);
        //public ICommand BottomRightCommand => new DelegateCommand(null, () => false);
        public Point BottomRightPoint { get; set; } = new Point(6300, 5800);

        public ICommand CutCommand => new DelegateCommand(
            () =>
            {
                Project.Cut(TopLeftPoint.X, TopLeftPoint.Y, BottomRightPoint.X, BottomRightPoint.Y);

                Red = Chanels.First(ch => ch.ID.StartsWith(Red.ID) && ch.ID != Red.ID);
                Green = Chanels.First(ch => ch.ID.StartsWith(Green.ID) && ch.ID != Green.ID);
                Blue = Chanels.First(ch => ch.ID.StartsWith(Blue.ID) && ch.ID != Blue.ID);

                Maps.Add(myImageFactory.CreateImage($"{Red.ID}_{Green.ID}_{Blue.ID}", new ImageParts(Red.Width, Red.Height, Red.Memory, Green.Memory, Blue.Memory)));
                CurrentMap = Maps.Last();

                this.RaisePropertyChanged(nameof(Chanels));
                this.RaisePropertyChanged(nameof(VariantsCommand));
            },
            () => Chanels.Any() && TopLeftPoint != Point.Empty && BottomRightPoint != Point.Empty);

        public byte Range { get; set; } = 3;
        public ICommand VariantsCommand => new DelegateCommand<byte?>(
            range =>
            {
                Project.CalculateVariantsWithStats(range.Value);
                this.RaisePropertyChanged(nameof(AsBytesCommand));
                this.RaisePropertyChanged(nameof(LoggedBytesCommand));
            },
            range => range.HasValue && range > 1 && range % 2 == 1);

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

                this.RaisePropertyChanged(nameof(SaveMapCommand));
            }
        }
        public ImageSource ImageSource { get; private set; }
        public ICommand SaveMapCommand => new DelegateCommand<Image>(
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

        public ICommand AsBytesCommand => new DelegateCommand(
            () =>
            {
                Project.ReclassToByte();
                Maps.Add(myImageFactory.CreateImage(nameof(Project.BYTES), new ImageParts(Project.BYTES.Width, Project.BYTES.Height, Project.BYTES.Memory)));
                CurrentMap = Maps.Last();
            },
            () => Project.RAW != null);
        public ICommand LoggedBytesCommand => new DelegateCommand(
            () =>
            {
                Project.ReclassToByteLog();
                Maps.Add(myImageFactory.CreateImage(nameof(Project.LOGGED), new ImageParts(Project.LOGGED.Width, Project.LOGGED.Height, Project.LOGGED.Memory)));
                CurrentMap = Maps.Last();
            },
            () => Project.RAW != null);

    }
}
