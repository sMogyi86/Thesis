using System.Collections.Generic;

namespace MARGO.BL
{
    public class Slicer
    {
        //public IReadOnlyDictionary<int, int> Slice(int num, int count)
        //{
        //    int nomalSize = num / count;
        //    int remain = num % count;

        //    return new Dictionary<int, int>(2)
        //    {
        //        { count-remain, nomalSize},
        //        { remain, nomalSize+1}
        //    };
        //}

        public IEnumerable<int> Slice(int num, int count)
        {
            int normalSize = num / count;
            int ext = normalSize + 1;
            int remain = num % count;

            var slices = new int[count];
            int i = 0;
            for (int j = 0; j < remain; j++)
                slices[i++] = ext;

            for (int j = 0; j < (count - remain); j++)
                slices[i++] = normalSize;

            return slices;
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
