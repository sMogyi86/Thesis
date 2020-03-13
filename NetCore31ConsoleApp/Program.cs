using MARGO.BL;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SandBoxConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var sw = Stopwatch.StartNew();
            var task = Testing();
            Console.WriteLine(sw.Elapsed);
            await task;
            Console.WriteLine(sw.Elapsed);

            Console.WriteLine("DONE");
        }


        private static async Task<int> Testing()
        {

            TaskCreationOptions.LongRunning


            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

            Parallel.For(0, 5, (i) =>
            {
                Thread.Sleep(1500);
            });

            tcs.SetResult(99);

            return await tcs.Task;
        }
             
    }
}
