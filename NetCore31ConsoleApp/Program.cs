using MARGO.BL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SandBoxConsoleApp
{
    class Program
    {
        private readonly static string _b40Path = @"D:\Segment\L5188027_02720060719_B40.TIF";
        private readonly static string _b30Path = @"D:\Segment\L5188027_02720060719_B30.TIF";
        private readonly static string _b20Path = @"D:\Segment\L5188027_02720060719_B20.TIF";
        private readonly static Project PROJECT = Project.Instance;

        static void Main(string[] args)
        {
            //PROJECT.Load(new string[3]
            //{
            //    _b40Path,
            //    _b30Path,
            //    _b20Path
            //});
            //PROJECT.Cut();
            //await PROJECT.CalculateVariantsWithStatsAsync();
            //PROJECT.ReclassToByteLog();
            //await PROJECT.FindMinimasAsync();

            //Console.WriteLine("DONE");

            IEnumerable<int> ints = new int[3] {1, 2, 3 };

            foreach (var item in ints.Take(100))
            {
                Console.WriteLine(item);
            }

            Console.ReadKey();
        }
    }
}