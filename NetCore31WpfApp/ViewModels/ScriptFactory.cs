using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace MARGO.ViewModels
{
    internal partial class ViewModel
    {
        public IScript Big()
        {
            var sampleGroups = new SampleGroupVM[]
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
                            })
                            //new SampleGroupVM("telep", 0xFFA9A9A9, CalcIndex, new Point[]
                            //{
                            //    new Point(2968,1896),
                            //    new Point(3000,1909),
                            //    new Point(2966,1941)
                            //})
                        };

            return new Script(nameof(Big), sampleGroups, new Dictionary<Step, Action>()
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
                {Step.Cut,
                    () => {
                        TopLeftPoint  = new Point(1800, 1600);
                        BottomRightPoint = new Point(6300, 5800);
                        CutCommand.Execute(WorkspacePrefix);
                    } },
                {Step.Variants, ()=> VariantsCommand.Execute(VariantsRange)},
                {Step.Minimas, ()=> MinimasCommand.Execute(MinimasRange)},
                {Step.Flood,
                    async ()=> {
                        StartBusy();
                        await Project.FloodAsync(myTokenSource.Token);
                        FloodTime = myStopwatch.Elapsed;

                        myStopwatch.Restart();
                        await Project.CreateSampleLayersAsync(SampleType, SampleType.ToString(), myTokenSource.Token);

                        this.ChangeFreshMap();
                        this.RaisePropertyChanged(nameof(Layers));
                        SampleTime = EndBusy();
                    } },
                {Step.Classify,
                    ()=> {
                        Groups.Clear();

                        foreach (var grp in sampleGroups)
                            Groups.Add(grp);

                        ClassifyCommand.Execute(SampleType);
                    } },
            });
        }

        public IScript Small()
        {
            var sampleGroups = new SampleGroupVM[]
                        {
                            new SampleGroupVM("viz", 0xFF000080, CalcIndex, new Point[]
                            {
                                new Point(306,205),
                                new Point(614,411)
                            }),
                            new SampleGroupVM("erdo1", 0xFFA52A2A, CalcIndex, new Point[]
                            {
                                new Point(478,216)
                            }),
                            new SampleGroupVM("erdo2", 0xFFB22222, CalcIndex, new Point[]
                            {
                                new Point(568,98)
                            }),
                            new SampleGroupVM("veg1", 0xFFFFA500, CalcIndex, new Point[]
                            {
                                new Point(950,522)
                            }),
                            new SampleGroupVM("veg2", 0xFFFF4500, CalcIndex, new Point[]
                            {
                                new Point(547,646)
                            }),
                            new SampleGroupVM("tal1", 0xFF66CDAA, CalcIndex, new Point[]
                            {
                                new Point(910,693)
                            }),
                            new SampleGroupVM("tal2", 0xFF2E8B57, CalcIndex, new Point[]
                            {
                                new Point(893,693)
                            }),
                            new SampleGroupVM("legelo", 0xFFF5DEB3, CalcIndex, new Point[]
                            {
                                new Point(497,364)
                            })
                            //new SampleGroupVM("telep", 0xFFA9A9A9, CalcIndex, new Point[]
                            //{
                            //    new Point(2968,1896),
                            //    new Point(3000,1909),
                            //    new Point(2966,1941)
                            //})
                        };

            return new Script(nameof(Small), sampleGroups, new Dictionary<Step, Action>()
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
                {Step.Cut,
                    () => {
                        TopLeftPoint  = new Point(3000, 4150);
                        BottomRightPoint = new Point(4350, 5000);
                        CutCommand.Execute(WorkspacePrefix);
                } },
                {Step.Variants, ()=> VariantsCommand.Execute(VariantsRange)},
                {Step.Minimas, ()=> MinimasCommand.Execute(MinimasRange)},
                {Step.Flood,
                    async ()=> {
                        StartBusy();
                        byte tmpLevelOfParallelism = LevelOfParallelism;
                        LevelOfParallelism = 1;
                        await Project.FloodAsync(myTokenSource.Token);
                        FloodTime = myStopwatch.Elapsed;

                        myStopwatch.Restart();
                        await Project.CreateSampleLayersAsync(SampleType, SampleType.ToString(), myTokenSource.Token);

                        this.ChangeFreshMap();
                        this.RaisePropertyChanged(nameof(Layers));
                        LevelOfParallelism = tmpLevelOfParallelism;
                        SampleTime = EndBusy();
                    } },
                {Step.Classify,
                    ()=> {
                        Groups.Clear();

                        foreach (var grp in sampleGroups)
                            Groups.Add(grp);

                        ClassifyCommand.Execute(SampleType);
                    } },
          });
        }
    }
}
