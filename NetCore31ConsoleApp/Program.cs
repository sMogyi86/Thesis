using StandardClassLibraryTestBL;
using System;
using System.Collections;
using System.Diagnostics;

namespace NetCore31ConsoleApp
{
    class Program
    {
        private readonly static string _imageName = @"c:\Users\z0040rwz\Documents\Private\OE NIK\_SzD\DATA\LE07_L1TP_188027_20011220_20170201_01_T2\LE07_L1TP_188027_20011220_20170201_01_T2_B3.TIF";
        private readonly static TestServices services = new TestServices();

        static void Main(string[] args)
        {
            //PrintTags();
            services.Testing(_imageName);

            //Jagged();
            //Multi();
            //Single();
        }

        static void PrintTags()
        {
            var tiffTagValues = services.GetTagValues(_imageName);

            foreach (var tag in tiffTagValues)
            {
                if (tag.Value != null)
                {
                    Console.WriteLine();
                    Console.WriteLine(tag.Key);

                    foreach (var value in tag.Value)
                    {
                        var typ = value.Value.GetType();
                        Console.WriteLine($"Type: {typ}");

                        var obj = value.Value;
                        if (obj is IEnumerable)
                        {
                            var enumerable = (IEnumerable)obj;
                            int c = 0;
                            foreach (var item in enumerable)
                            {
                                c++;
                                //Console.WriteLine(item);
                            }
                            Console.WriteLine($"Count: {c}");
                        }
                        else
                        {
                            Console.WriteLine($"Value: {obj}");
                        }

                    }
                }
            }
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
