using System.Runtime.Intrinsics.X86;
using System.Numerics;
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
            Testing();

            //Console.ReadKey();
        }

        private static void Testing()
        {
            //bool isHardwareAccelerated = Vector.IsHardwareAccelerated;
            //Console.WriteLine(isHardwareAccelerated);
            //Console.WriteLine(Vector<byte>.Count);

            //Console.WriteLine(Aes.IsSupported);
            //Console.WriteLine(Avx.IsSupported);
            ////Console.WriteLine(Avx2.IsSupported);
            ////Console.WriteLine(Bmi1.IsSupported);
            ////Console.WriteLine(Bmi2.IsSupported);
            ////Console.WriteLine(Fma.IsSupported);
            ////Console.WriteLine(Lzcnt.IsSupported);
            //Console.WriteLine(Pclmulqdq.IsSupported);
            //Console.WriteLine(Popcnt.IsSupported);
            //Console.WriteLine(Sse.IsSupported);
            //Console.WriteLine(Sse2.IsSupported);
            //Console.WriteLine(Sse3.IsSupported);
            //Console.WriteLine(Sse41.IsSupported);
            //Console.WriteLine(Sse42.IsSupported);
            //Console.WriteLine(Ssse3.IsSupported);



            int byteVectorCount = Vector<byte>.Count;
            int shortVectorcount = Vector<short>.Count;
            int ushortVectorCount = Vector<ushort>.Count;
            
            short[] a = new short[shortVectorcount];
            short[] b = new short[shortVectorcount];
            for (int i = 0; i < 3; i++)
            {
                a[i] = 0;
            }

            b[0] = 8;
            b[1] = 8;
            b[2] = 31;

            var av = new Vector<short>(a);
            Console.WriteLine(av);
            var bv = new Vector<short>(b);
            Console.WriteLine(bv);


            var delta = Vector.AsVectorUInt16(Vector.Subtract(av, bv));
            double distance = Math.Sqrt(Vector.Dot(Vector.Multiply(delta, delta), Vector<ushort>.One));
            Console.WriteLine(distance);
        }
    }
}