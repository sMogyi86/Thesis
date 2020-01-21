using StandardClassLibrary_TestBL;
using System;
using System.Collections;

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
    }
}
