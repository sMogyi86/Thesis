using MARGO.BL;
using MARGO.BL.Img;
using MARGO.Common;
using MARGO.UIServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MARGO.ViewModels
{
    class ViewModel : ObservableBase
    {
        #region Services
        private readonly IImageFactory myImageFactory = Services.GetImageFactory();
        private readonly MsgBox myUIServices = new MsgBox();
        #endregion



        private readonly Stopwatch myStopwatch = new Stopwatch();
        private Project Project => Project.Instance;
        public bool IsBusy { get; set; } = false;
        public byte LevelOfParallelism { get { return Project.LevelOfParallelism; } set { Project.LevelOfParallelism = value; } }



        public IEnumerable<RasterLayer> Layers => Project.Layers;
        public ICommand LoadCommand => new DelegateCommandAsync(
            async () =>
            {
                var ids = myUIServices.GetLayerFiles();

                if (ids.Any())
                {
                    StartBusy();
                    // https://stackoverflow.com/questions/54594297/wrapping-slow-synchronous-i-o-for-asynchronous-ui-consumption
                    await Task.Run(() => { Project.Load(ids); });
                    LoadTime = EndBusy();

                    this.RaisePropertyChanged(nameof(Layers));
                    this.RaisePropertyChanged(nameof(CutCommand));
                }
            });
        public string LoadTime { get; set; }


        public RasterLayer Red { get; set; }
        public RasterLayer Green { get; set; }
        public RasterLayer Blue { get; set; }
        public ICommand ComposeCommand => new DelegateCommandAsync(
            async () =>
            {
                StartBusy();
                var map = await Task.Run(() => myImageFactory.CreateImage($"{Red.ID}_{Green.ID}_{Blue.ID}", new ImageParts(Red.Width, Red.Height, Red.Memory, Green.Memory, Blue.Memory)));
                ComposeTime = EndBusy();

                Maps.Add(map);

                CurrentMap = Maps.Last();
            },
            () => Red != null && Green != null && Blue != null);
        public string ComposeTime { get; set; }


        //public ICommand TopLeftCommand =>  new DelegateCommand(null, () => false);
        public Point TopLeftPoint { get; set; } = new Point(1800, 1600);
        //public ICommand BottomRightCommand => new DelegateCommand(null, () => false);
        public Point BottomRightPoint { get; set; } = new Point(6300, 5800);
        public string CutNamePrefix { get; set; }
        public ICommand CutCommand => new DelegateCommandAsync<string>(
            async (prefix) =>
            {
                StartBusy();
                await Project.Cut(TopLeftPoint.X, TopLeftPoint.Y, BottomRightPoint.X, BottomRightPoint.Y, prefix);
                CutTime = EndBusy();

                this.ChangeFreshMap();
                this.RaisePropertyChanged(nameof(Layers));
                this.RaisePropertyChanged(nameof(VariantsCommand));
            },
            _ => Layers.Any() && TopLeftPoint != Point.Empty && BottomRightPoint != Point.Empty);
        public string CutTime { get; set; }


        public byte VariantsRange { get; set; } = 3;
        public ICommand VariantsCommand => new DelegateCommandAsync<byte>(
            async range =>
            {
                StartBusy();
                await Project.CalculateVariantsWithStatsAsync(range);
                VariantsTime = EndBusy();

                Maps.Add(myImageFactory.CreateImage(nameof(Project.BYTES), new ImageParts(Project.BYTES.Width, Project.BYTES.Height, Project.BYTES.Memory)));
                Maps.Add(myImageFactory.CreateImage(nameof(Project.LOGGED), new ImageParts(Project.LOGGED.Width, Project.LOGGED.Height, Project.LOGGED.Memory)));
                CurrentMap = Maps.Last();

                this.RaisePropertyChanged(nameof(MinimasCommand));
            },
            range => Project.CanCalcVariants && range > 1 && range % 2 == 1);
        public string VariantsTime { get; set; }


        public byte MinimasRange { get; set; } = 3;
        public ICommand MinimasCommand => new DelegateCommandAsync<byte>(
            async (range) =>
            {
                StartBusy();
                await Project.FindMinimasAsync(range);
                MinimasTime = EndBusy();

                Maps.Add(myImageFactory.CreateImage(nameof(Project.MINIMAS), new ImageParts(Project.LOGGED.Width, Project.LOGGED.Height, Project.MINIMAS)));
                CurrentMap = Maps.Last();

                this.RaisePropertyChanged(nameof(FloodCommand));
            },
            range => Project.LOGGED != null && range > 1 && range % 2 == 1);
        public string MinimasTime { get; set; }


        public ICommand FloodCommand => new DelegateCommandAsync(
            async () =>
            {
                StartBusy();
                await Project.FloodAsync();
                FloodTime = EndBusy();
            },
            () => Project.CanFlood);
        public string FloodTime { get; set; }


        public SampleType SampleType { get; set; } = SampleType.Mean;
        public ICommand CreateSampleCommand => new DelegateCommandAsync<SampleType>(
            async (smapleType) =>
            {
                StartBusy();
                await Project.CreateSampleLayersAsync(smapleType, SampleType.ToString());
                SampleTime =  FloodTime = EndBusy();

                this.ChangeFreshMap();
                this.RaisePropertyChanged(nameof(Layers));
            });
        public string  SampleTime { get; set; }


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
        public ICommand SaveMapCommand => new DelegateCommandAsync<Image>(
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

        private void ChangeFreshMap()
        {
            Red = Layers.First(ch => ch.ID.EndsWith(Red.ID) && ch.ID != Red.ID);
            Green = Layers.First(ch => ch.ID.EndsWith(Green.ID) && ch.ID != Green.ID);
            Blue = Layers.First(ch => ch.ID.EndsWith(Blue.ID) && ch.ID != Blue.ID);

            Maps.Add(myImageFactory.CreateImage($"{Red.ID}_{Green.ID}_{Blue.ID}", new ImageParts(Red.Width, Red.Height, Red.Memory, Green.Memory, Blue.Memory)));
            CurrentMap = Maps.Last();

            this.RaisePropertyChanged(nameof(SaveMapCommand));
        }

        private void StartBusy()
        {
            IsBusy = true;
            myStopwatch.Start();
        }

        private string EndBusy()
        {
            myStopwatch.Stop();
            IsBusy = false;
            return myStopwatch.Elapsed.ToString(@"h\:mm\:ss\.fff");
        }
    }
}
