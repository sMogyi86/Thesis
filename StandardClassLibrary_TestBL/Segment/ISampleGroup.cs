using System.Collections.Generic;

namespace MARGO.BL.Segment
{
    public interface ISampleGroup
    {
        uint ID { get; }
        IEnumerable<int> Indexes { get; }
    }
}