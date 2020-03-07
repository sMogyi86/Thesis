using StandardClassLibraryTestBL;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NetCore31ConsoleApp
{
    class Program
    {
        private readonly static string _imageName = @"D:\Segment\B30.TIFF"; // @"c:\Users\z0040rwz\Documents\Private\OE NIK\_SzD\DATA\LE07_L1TP_188027_20011220_20170201_01_T2\LE07_L1TP_188027_20011220_20170201_01_T2_B3.TIF";
        private readonly static string _b50Path = @"D:\Segment\L5188027_02720060719_B50.TIF";
        private readonly static string _b40Path = @"D:\Segment\L5188027_02720060719_B40.TIF";
        private readonly static string _b30Path = @"D:\Segment\L5188027_02720060719_B30.TIF";
        private readonly static string _b20Path = @"D:\Segment\L5188027_02720060719_B20.TIF";
        private readonly static string _b10Path = @"D:\Segment\L5188027_02720060719_B10.TIF";
        private readonly static TestServices testServices = new TestServices();
        private readonly static IIOService iOService = Services.GetIO();
        private readonly static ICompositeFactory compositeFactory = Services.GetCompositeFactory();        

        static void Main(string[] args)
        {
            Console.WriteLine((double)(double.Epsilon + byte.MaxValue));

            Console.WriteLine("DONE");
        }

        static void CompsiteTest()
        {
            using (FileStream fileStream = new FileStream(@"D:\Segment\New folder\alma.tiff", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                //IRaster r = iOService.Read(_b50Path);
                IRasterLayer r = iOService.Load(_b40Path);
                IRasterLayer g = iOService.Load(_b30Path);
                IRasterLayer b = iOService.Load(_b20Path);

                var compositeParts = new TiffParts(r.Width, r.Height, r.Data, g.Data, b.Data);

                using (var c = compositeFactory.CreateTiff(compositeParts))
                {
                    //c.Composite.CopyTo(fileStream);

                    fileStream.Flush();
                }
            }
        }

        static void PrintTags(string tiffPath)
        {
            //var tiffTagValues = testServices.GetTagValues(tiffPath);

            //foreach (var tag in tiffTagValues)
            //{
            //    if (tag.Value != null)
            //    {
            //        Console.WriteLine();
            //        Console.WriteLine(tag.Key);

            //        foreach (var value in tag.Value)
            //        {
            //            var typ = value.Value.GetType();
            //            Console.WriteLine($"Type: {typ}");

            //            var obj = value.Value;
            //            if (obj is IEnumerable)
            //            {
            //                var enumerable = (IEnumerable)obj;
            //                int c = 0;
            //                foreach (var item in enumerable)
            //                {
            //                    c++;
            //                    //Console.WriteLine(item);
            //                }
            //                Console.WriteLine($"Count: {c}");
            //            }
            //            else
            //            {
            //                Console.WriteLine($"Value: {obj}");
            //            }

            //        }
            //    }
            //}
        }

        const string Format = "{0,7:0.000} ";
        static void Jagged()
        {
            const int dim = 100;
            for (var passes = 0; passes < 10; passes++)
            {
                var timer = new Stopwatch();
                timer.Start();
                var jagged = new int[dim][][];
                for (var i = 0; i < dim; i++)
                {
                    jagged[i] = new int[dim][];
                    for (var j = 0; j < dim; j++)
                    {
                        jagged[i][j] = new int[dim];
                        for (var k = 0; k < dim; k++)
                        {
                            jagged[i][j][k] = i * j * k;
                        }
                    }
                }
                timer.Stop();
                Console.Write(Format,
                    (double)timer.ElapsedTicks / TimeSpan.TicksPerMillisecond);
            }
            Console.WriteLine();
        }

        static void Multi()
        {
            const int dim = 100;
            for (var passes = 0; passes < 10; passes++)
            {
                var timer = new Stopwatch();
                timer.Start();
                var multi = new int[dim, dim, dim];
                for (var i = 0; i < dim; i++)
                {
                    for (var j = 0; j < dim; j++)
                    {
                        for (var k = 0; k < dim; k++)
                        {
                            multi[i, j, k] = i * j * k;
                        }
                    }
                }
                timer.Stop();
                Console.Write(Format,
                    (double)timer.ElapsedTicks / TimeSpan.TicksPerMillisecond);
            }
            Console.WriteLine();
        }

        static void Single()
        {
            const int dim = 100;
            for (var passes = 0; passes < 10; passes++)
            {
                var timer = new Stopwatch();
                timer.Start();
                var single = new int[dim * dim * dim];
                for (var i = 0; i < dim; i++)
                {
                    for (var j = 0; j < dim; j++)
                    {
                        for (var k = 0; k < dim; k++)
                        {
                            single[i * dim * dim + j * dim + k] = i * j * k;
                        }
                    }
                }
                timer.Stop();
                Console.Write(Format,
                    (double)timer.ElapsedTicks / TimeSpan.TicksPerMillisecond);
            }
            Console.WriteLine();
        }
    }
}
