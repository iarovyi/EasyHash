namespace EasyHash.Benchmark.Benchmarks.Targets
{
    using System.Collections.Generic;
    using System.Linq;
    using MSIL;

    internal sealed class HashedWithComplexEasyHashRegistration
    {
        public int Number { get; set; }

        public string Text { get; set; }

        public IEnumerable<int> Numbers { get; set; }

        static HashedWithComplexEasyHashRegistration()
        {
            FastEasyHash<HashedWithComplexEasyHashRegistration>
                .Register(r => r
                    .WithPrimes(17, 23)
                    .Skip(f => f.Number)
                    .For(f => f.Numbers, (ob, hash) => ob.Numbers.Aggregate(hash, (i, i1) => (i * 23) ^ i1.GetHashCode()))
                    .For(f => f.Text, (ob, hash) => (hash * 23) ^ ob.Text.GetHashCode())
                    .ExcludeCollectionItems());
        }

        public override int GetHashCode() => FastEasyHash<HashedWithComplexEasyHashRegistration>.GetHashCode(this);
        public override bool Equals(object obj) => FastEasyHash<HashedWithComplexEasyHashRegistration>.Equals(this, obj);
    }
}
