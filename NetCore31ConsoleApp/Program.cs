//using BitMiracle.LibTiff.Classic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace SandBoxConsoleApp
{
    class Program
    {
        private readonly static string _imageName = @"D:\MyDocs\SkyDrive\MogyiSharePublic\Results\class.tif";
        //private readonly static string _imageName = @"D:\Segment\L5188027_02720060719_B30.TIF";

        static void Main(string[] args)
        {
                       string laodPath = Path.GetDirectoryName((typeof(Program)).Assembly.Location);
            Console.WriteLine(laodPath);
            Console.ReadKey();
        }

        //public static void TagTest()
        //{
        //    var tiffTagValues = GetTagValues(_imageName);

        //    foreach (var tag in tiffTagValues)
        //    {
        //        if (tag.Value != null)
        //        {
        //            Console.WriteLine();
        //            Console.WriteLine(tag.Key);

        //            foreach (var value in tag.Value)
        //            {
        //                var typ = value.Value.GetType();
        //                Console.WriteLine($"Type: {typ}");

        //                var obj = value.Value;
        //                if (obj is IEnumerable)
        //                {
        //                    var enumerable = (IEnumerable)obj;
        //                    int c = 0;
        //                    foreach (var item in enumerable)
        //                    {
        //                        c++;
        //                        //Console.WriteLine(item);
        //                    }
        //                    Console.WriteLine($"Count: {c}");
        //                }
        //                else
        //                {
        //                    Console.WriteLine($"Value: {obj}");
        //                }

        //            }
        //        }
        //    }
        //}

        //public static IReadOnlyDictionary<TiffTag, FieldValue[]> GetTagValues(string imageName)
        //{
        //    var tiff = Tiff.Open(imageName, "r");

        //    Dictionary<TiffTag, FieldValue[]> tiffTagValues = new Dictionary<TiffTag, FieldValue[]>();

        //    Console.WriteLine(tiff.NumberOfDirectories());

        //    tiff.WriteDirectory();

        //    do
        //    {
        //        foreach (TiffTag tiffTag in System.Enum.GetValues(typeof(TiffTag)).Cast<TiffTag>())
        //        {
        //            tiffTagValues[tiffTag] = tiff.GetField(tiffTag);
        //        }
        //    } while (tiff.ReadDirectory());

            

        //    return tiffTagValues;
        //}

        //private static void Testing()
        //{
        //    //bool isHardwareAccelerated = Vector.IsHardwareAccelerated;
        //    //Console.WriteLine(isHardwareAccelerated);
        //    //Console.WriteLine(Vector<byte>.Count);

        //    //Console.WriteLine(Aes.IsSupported);
        //    //Console.WriteLine(Avx.IsSupported);
        //    ////Console.WriteLine(Avx2.IsSupported);
        //    ////Console.WriteLine(Bmi1.IsSupported);
        //    ////Console.WriteLine(Bmi2.IsSupported);
        //    ////Console.WriteLine(Fma.IsSupported);
        //    ////Console.WriteLine(Lzcnt.IsSupported);
        //    //Console.WriteLine(Pclmulqdq.IsSupported);
        //    //Console.WriteLine(Popcnt.IsSupported);
        //    //Console.WriteLine(Sse.IsSupported);
        //    //Console.WriteLine(Sse2.IsSupported);
        //    //Console.WriteLine(Sse3.IsSupported);
        //    //Console.WriteLine(Sse41.IsSupported);
        //    //Console.WriteLine(Sse42.IsSupported);
        //    //Console.WriteLine(Ssse3.IsSupported);



        //    int byteVectorCount = Vector<byte>.Count;
        //    int shortVectorcount = Vector<short>.Count;
        //    int ushortVectorCount = Vector<ushort>.Count;

        //    short[] a = new short[shortVectorcount];
        //    short[] b = new short[shortVectorcount];
        //    for (int i = 0; i < 3; i++)
        //    {
        //        a[i] = 0;
        //    }

        //    b[0] = 8;
        //    b[1] = 8;
        //    b[2] = 31;

        //    var av = new Vector<short>(a);
        //    Console.WriteLine(av);
        //    var bv = new Vector<short>(b);
        //    Console.WriteLine(bv);


        //    var delta = Vector.AsVectorUInt16(Vector.Subtract(av, bv));
        //    double distance = Math.Sqrt(Vector.Dot(Vector.Multiply(delta, delta), Vector<ushort>.One));
        //    Console.WriteLine(distance);
        //}
    }
}