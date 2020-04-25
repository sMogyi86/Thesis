using MARGO.BL;
using MARGO.BL.Img;
using MARGO.Common;
using MARGO.UIServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MARGO.ViewModels
{
    class ViewModel : ObservableBase, IUIHelper, IHaveScript
    {
        #region Services
        private readonly IImageFactory myImageFactory = MyServices.GetImageFactory();
        private readonly MsgBox myUIServices = new MsgBox();
        #endregion



        private readonly Script myScript;
        public ViewModel()
        {
            myScript = new Script(new Dictionary<Step, Action>()
            {
                {Step.Load,
                    async ()=> {
                        StartBusy();
                        string laodPath;
#if DEBUG
                        laodPath = @"D:\Segment\";
#else
                        laodPath = this.GetType().Assembly.Location;
#endif
                        await Load(new DirectoryInfo(laodPath).EnumerateFiles("*.TIF").Select(fi => fi.FullName));
                        LoadTime = EndBusy();
                    } },
                {Step.Compose,
                    ()=> {
                        Red = Project.Layers.FirstOrDefault(l => l.ID.Contains("B40"));
                        Green= Project.Layers.FirstOrDefault(l => l.ID.Contains("B50"));
                        Blue= Project.Layers.FirstOrDefault(l => l.ID.Contains("B30"));
                        //ComposeCommand.Execute(null);
                        IsBusy = true; IsBusy = false;
                    } },
                {Step.Cut, () => CutCommand.Execute(WorkspacePrefix)},
                {Step.Variants, ()=> VariantsCommand.Execute(VariantsRange)},
                {Step.Minimas, ()=> MinimasCommand.Execute(MinimasRange)},
                {Step.Flood,
                    async ()=> {
                        StartBusy();
                        await Project.FloodAsync();
                        FloodTime = myStopwatch.Elapsed;

                        myStopwatch.Restart();
                        await Project.CreateSampleLayersAsync(SampleType, SampleType.ToString());

                        this.ChangeFreshMap();
                        this.RaisePropertyChanged(nameof(Layers));
                        SampleTime = EndBusy();
                    } },
                {Step.Classify,
                    ()=> {
                        var groups = new SampleGroupVM[]
                        {
                            new SampleGroupVM("viz1", 0xFF1E90FF, CalcIndex, new Point[]
                            {
                                new Point(132,3946),
                                new Point(334,3711),
                                new Point(490,3796),
                                new Point(346,3903)
                            }),
                            new SampleGroupVM("viz2", 0xFF000080, CalcIndex, new Point[]
                            {
                                new Point(1809,2955),
                                new Point(1507,2773),
                                new Point(906,2654),
                                new Point(1161,1353)
                            }),
                            new SampleGroupVM("erdo1", 0xFFA52A2A, CalcIndex, new Point[]
                            {
                                new Point(2842,1090),
                                new Point(2479,1333)
                            }),
                            new SampleGroupVM("erdo2", 0xFFB22222, CalcIndex, new Point[]
                            {
                                new Point(1202,2213),
                                new Point(956,2279)
                            }),
                            new SampleGroupVM("veg1", 0xFFFFA500, CalcIndex, new Point[]
                            {
                                new Point(1443,3179),
                                new Point(1558,2504),
                                new Point(2147,3074)
                            }),
                            new SampleGroupVM("veg2", 0xFFFF4500, CalcIndex, new Point[]
                            {
                                new Point(1641,2346),
                                new Point(2218,1775),
                                new Point(1098,3902)
                            }),
                            new SampleGroupVM("tal1", 0xFF66CDAA, CalcIndex, new Point[]
                            {
                                new Point(2251,3118),
                                new Point(766,3397),
                                new Point(1998,1895)
                            }),
                            new SampleGroupVM("tal2", 0xFF2E8B57, CalcIndex, new Point[]
                            {
                                new Point(1875,1742),
                                new Point(1620,2245),
                                new Point(2043,2648)
                            }),
                            new SampleGroupVM("legelo", 0xFFF5DEB3, CalcIndex, new Point[]
                            {
                                new Point(1528,3258),
                                new Point(1672,3232)
                            }),
                            new SampleGroupVM("telep", 0xFFA9A9A9, CalcIndex, new Point[]
                            {
                                new Point(2968,1896),
                                new Point(3000,1909),
                                new Point(2966,1941)
                            })
                        };

                        foreach (var grp in groups)
                            Groups.Add(grp);

                        ClassifyCommand.Execute(SampleType);
                    } },
            });

            this.PropertyChanged +=
                (sender, e) =>
                {
                    if (e.PropertyName == nameof(IsBusy) && !IsBusy)
                        myScript.DoNextStep();
                };
        }
        public void SetLastStep(Step step) => myScript.SetLastStep(step);



        private readonly Stopwatch myStopwatch = new Stopwatch();
        private Project Project => Project.Instance;
        public bool IsBusy { get; set; } = false;
        public byte LevelOfParallelism { get { return Project.LevelOfParallelism; } set { Project.LevelOfParallelism = value; } }
        public ICommand AutoPlayCommand => new DelegateCommand(() => myScript.StartToPlay());



        public IEnumerable<RasterLayer> Layers => Project.Layers;
        public ICommand LoadCommand => new DelegateCommandAsync(
            async () =>
            {
                var ids = myUIServices.GetLayerFiles();

                StartBusy();
                await Load(ids);
            })
        { Finaly = () => LoadTime = EndBusy() };
        private async Task Load(IEnumerable<string> ids)
        {
            if (ids.Any())
            {
                // https://stackoverflow.com/questions/54594297/wrapping-slow-synchronous-i-o-for-asynchronous-ui-consumption
                await Task.Run(() => { Project.Load(ids); });

                this.RaisePropertyChanged(nameof(Layers));
                this.RaisePropertyChanged(nameof(CutCommand));
            }
        }
        public TimeSpan LoadTime { get; set; }


        public RasterLayer Red { get; set; }
        public RasterLayer Green { get; set; }
        public RasterLayer Blue { get; set; }
        public ICommand ComposeCommand => new DelegateCommandAsync(
            async () =>
            {
                StartBusy();
                var map = await Task.Run(() => myImageFactory.CreateImage($"{Red.ID}_{Green.ID}_{Blue.ID}", new ImageParts(Red.Width, Red.Height, Red.Memory, Green.Memory, Blue.Memory)));

                Maps.Add(map);

                CurrentMap = Maps.Last();
            },
            () => Red != null && Green != null && Blue != null)
        { Finaly = () => ComposeTime = EndBusy() };
        public TimeSpan ComposeTime { get; set; }


        public ICommand TopLeftCommand => new DelegateCommand(() => Handle = p => TopLeftPoint = p, () => Red != null && Green != null && Blue != null);
        private Point? TopLeftPoint { get; set; } = new Point(1800, 1600);
        public double? TopLeftX => TopLeftPoint?.X;
        public double? TopLeftY => TopLeftPoint?.Y;
        public ICommand BottomRightCommand => new DelegateCommand(() => Handle = p => BottomRightPoint = p, () => Red != null && Green != null && Blue != null);
        private Point? BottomRightPoint { get; set; } = new Point(6300, 5800);
        public double? BottomRightX => BottomRightPoint?.X;
        public double? BottomRightY => BottomRightPoint?.Y;
        public string WorkspacePrefix { get; set; } = string.Empty;
        public ICommand CutCommand => new DelegateCommandAsync<string>(
            async (prefix) =>
            {
                StartBusy();
                await Project.Cut((int)TopLeftPoint.Value.X, (int)TopLeftPoint.Value.Y, (int)BottomRightPoint.Value.X, (int)BottomRightPoint.Value.Y, prefix);

                this.ChangeFreshMap();
                this.RaisePropertyChanged(nameof(Layers));
                this.RaisePropertyChanged(nameof(VariantsCommand));
            },
            _ => Layers.Any() && TopLeftPoint != null && BottomRightPoint != null)
        { Finaly = () => CutTime = EndBusy() };
        public TimeSpan CutTime { get; set; }


        public byte VariantsRange { get; set; } = 3;
        public ICommand VariantsCommand => new DelegateCommandAsync<byte>(
            async range =>
            {
                StartBusy();
                await Project.CalculateVariantsWithStatsAsync(range);

                Maps.Add(myImageFactory.CreateImage(nameof(Project.BYTES), new ImageParts(Project.BYTES.Width, Project.BYTES.Height, Project.BYTES.Memory)));
                Maps.Add(myImageFactory.CreateImage(nameof(Project.LOGGED), new ImageParts(Project.LOGGED.Width, Project.LOGGED.Height, Project.LOGGED.Memory)));
                CurrentMap = Maps.Last();

                this.RaisePropertyChanged(nameof(MinimasCommand));
            },
            range => Project.CanCalcVariants && range > 1 && range % 2 == 1)
        { Finaly = () => VariantsTime = EndBusy() };
        public TimeSpan VariantsTime { get; set; }


        public byte MinimasRange { get; set; } = 5;
        public ICommand MinimasCommand => new DelegateCommandAsync<byte>(
            async (range) =>
            {
                StartBusy();
                await Project.FindMinimasAsync(range);

                Maps.Add(myImageFactory.CreateImage(nameof(Project.MINIMAS), new ImageParts(Project.LOGGED.Width, Project.LOGGED.Height, Project.MINIMAS)));
                CurrentMap = Maps.Last();

                this.RaisePropertyChanged(nameof(FloodCommand));
            },
            range => Project.LOGGED != null && range > 1 && range % 2 == 1)
        { Finaly = () => MinimasTime = EndBusy() };
        public TimeSpan MinimasTime { get; set; }
        public string MinimasCount { get; set; } = "TODO";


        public ICommand FloodCommand => new DelegateCommandAsync(
            async () =>
            {
                StartBusy();
                await Project.FloodAsync();
            },
            () => Project.CanFlood)
        { Finaly = () => FloodTime = EndBusy() };
        public TimeSpan FloodTime { get; set; }


        public SampleType SampleType { get; set; } = SampleType.Mean;
        public ICommand CreateSampleCommand => new DelegateCommandAsync<SampleType>(
            async (smapleType) =>
            {
                StartBusy();
                await Project.CreateSampleLayersAsync(smapleType, smapleType.ToString());

                this.ChangeFreshMap();
                this.RaisePropertyChanged(nameof(Layers));
            })
        { Finaly = () => SampleTime = EndBusy() };
        public TimeSpan SampleTime { get; set; }


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


        private DelegateCommand<Point?> myClickHandlerCommand;
        public Action<Point?> Handle
        {
            get
            {
                return myClickHandlerCommand is null ? (null as Action<Point?>)
                  : point => myClickHandlerCommand.Execute(point);
            }
            set
            {
                if (value is null)
                {
                    myClickHandlerCommand = null;
                    EndBusy();
                }
                else
                {
                    StartBusy();

                    myClickHandlerCommand = new DelegateCommand<Point?>(
                    point =>
                    {
                        EndBusy();
                        value.Invoke(point);
                        Handle = null;
                    });
                }
            }
        }
        public ICommand ResetClickHandlerCommand => new DelegateCommand(() => Handle = null);


        public uint CurrentColor { get; set; }
        public string CurrentName { get; set; }
        public ICommand CreateGroupCommand => new DelegateCommand(
            () =>
            {
                Groups.Add(new SampleGroupVM(CurrentName, CurrentColor, CalcIndex));
                CurrentColor = 0;
                CurrentName = string.Empty;
            },
            () => 0 != CurrentColor && !string.IsNullOrEmpty(CurrentName) && !Groups.Any(g => CurrentColor == g.Color));
        public ObservableCollection<SampleGroupVM> Groups { get; } = new ObservableCollection<SampleGroupVM>();
        private SampleGroupVM myCurrentGroup;
        public SampleGroupVM CurrentGroup
        {
            get { return myCurrentGroup; }
            set
            {
                myCurrentGroup = value;
                this.RaisePropertyChanged(nameof(DeleteGroupCommand));
                this.RaisePropertyChanged(nameof(AddToGroupCommand));
            }
        }
        public ICommand DeleteGroupCommand => new DelegateCommand<SampleGroupVM>(
            grp =>
            {
                Groups.Remove(grp);
                this.RaisePropertyChanged(nameof(ClassifyCommand));
            }, grp => grp != null);
        public ICommand AddToGroupCommand => new DelegateCommand<SampleGroupVM>(
            grp => Handle =
                p =>
                {
                    grp.AddPoint(p);
                    this.RaisePropertyChanged(nameof(ClassifyCommand));
                }, grp => grp != null);
        public ICommand ClassifyCommand => new DelegateCommandAsync<SampleType>(
            async sType =>
            {
                StartBusy();
                await Project.ClassifyAsync(sType, Groups);

                Maps.Add(myImageFactory.CreateImage($"{nameof(Project.CLASSIFIEDIMAGE)}_RAW", new ImageParts(CurrentMap.Parts.Width, CurrentMap.Parts.Height, Project.CLASSIFIEDIMAGE)));
                CurrentMap = Maps.Last();
                Maps.Add(myImageFactory.CreateImage($"{nameof(Project.CLASSIFIEDIMAGE)}_COLORED", new ImageParts(CurrentMap.Parts.Width, CurrentMap.Parts.Height, Project.CLASSIFIEDIMAGE, Project.ColorMapping)));
            },
            _ => Groups.Any())
                            {
                                Finaly = () =>
                                {
                                    ClassifyTime = EndBusy();
                                    RaiseForAll();
                                }
                            };
        public TimeSpan ClassifyTime { get; set; }


        //public void TimerElapsedAt(Point? point) { }

        private void ChangeFreshMap()
        {
            Red = Layers.First(ch => ch.ID.EndsWith(Red.ID) && ch.ID != Red.ID);
            Green = Layers.First(ch => ch.ID.EndsWith(Green.ID) && ch.ID != Green.ID);
            Blue = Layers.First(ch => ch.ID.EndsWith(Blue.ID) && ch.ID != Blue.ID);

            Maps.Add(myImageFactory.CreateImage($"{Red.ID}_{Green.ID}_{Blue.ID}", new ImageParts(Red.Width, Red.Height, Red.Memory, Green.Memory, Blue.Memory)));
            CurrentMap = Maps.Last();

            this.RaisePropertyChanged(nameof(SaveMapCommand));
        }

        private int CalcIndex(Point p)
            => currentMap.Parts.Width * (int)p.Y + (int)p.X;

        private void StartBusy()
        {
            IsBusy = true;
            myStopwatch.Restart();
        }

        private TimeSpan EndBusy()
        {
            myStopwatch.Stop();
            IsBusy = false;
            return myStopwatch.Elapsed;
        }
    }
}