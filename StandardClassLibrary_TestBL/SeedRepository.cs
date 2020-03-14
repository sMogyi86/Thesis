using System;
using System.Collections.Generic;
using System.Text;

namespace MARGO.BL
{
    public class SeedRepository
    {
        public ReadOnlyMemory<int> Minimas { get; private set; }

        private readonly Lazy<IReadOnlyDictionary<int, Seed>> myLazy;
        public IReadOnlyDictionary<int, Seed> Seeds => myLazy.Value;

        public SeedRepository(ReadOnlyMemory<int> minimas)
        {
            Minimas = minimas;
            myLazy = new Lazy<IReadOnlyDictionary<int, Seed>>(this.Initailize);
        }

        private IReadOnlyDictionary<int, Seed> Initailize()
        {
            Dictionary<int, Seed> seeds = new Dictionary<int, Seed>();

            foreach (var i in Minimas.Span)
                if (i > -1)
                    seeds[i] = new Seed(i);

            return seeds;
        }
    }
}
