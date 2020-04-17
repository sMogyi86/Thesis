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

        static async Task Main(string[] args)
        {
            var pc = Environment.ProcessorCount;
            Console.WriteLine(pc);

            var tasks = new Task[pc];

            Random rnd = new Random();
            for (int i = 0; i < pc; i++)
            {
                int num = rnd.Next(0, pc/2);

                tasks[i] = Task.Run(() => StringLockTest(num));
            }
            await Task.WhenAll(tasks);

            Console.ReadKey();
        }

        private static void StringLockTest(int num)
        {
            var thread = Thread.CurrentThread;                
            Console.WriteLine($"{thread.ManagedThreadId} ENTERS [{num}]");
            lock (num.ToString())
            {
                Thread.Sleep(250);
                Console.WriteLine($"{thread.ManagedThreadId} EXECUTES  [{num}]");
            }
            Console.WriteLine($"{thread.ManagedThreadId} EXITS  [{num}]");
        }
    }
}