using System;
using System.Collections.Generic;
using System.Text;

namespace MARGO.BL
{
    public class Slicer
    {
        public IReadOnlyDictionary<int, int> Slice(int num, byte count)
        {
            int nomalSize = num / count;
            int remain = num % count;

            return new Dictionary<int, int>(2)
            {
                { count-remain, nomalSize},
                { remain, nomalSize+1}
            };
        }


    }

    //public class CalculateVariantsParam
    //{
    //    public byte FilterMatrixSize { get; }
    //    public int Width { get; set; }
    //    public int Start { get; }
    //    public int Length { get; }
    //    //public double Weight { get; }

    //    public CalculateVariantsParam(byte filterMatrixSize, int width, int start, int length)// , double weight)
    //    {
    //        FilterMatrixSize = filterMatrixSize;
    //        Width = width;
    //        Start = start;
    //        Length = length;
    //        //Weight = weight;
    //    }
    //}
}
