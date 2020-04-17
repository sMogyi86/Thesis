using System.Collections.Generic;

namespace MARGO.BL.Segment
{
    public interface ISampleGroup
    {
        int ID { get; }
        IEnumerable<int> Indexes { get; }
    }
}