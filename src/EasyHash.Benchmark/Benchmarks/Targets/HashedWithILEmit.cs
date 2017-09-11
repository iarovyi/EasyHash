namespace EasyHash.Benchmark.Benchmarks.Targets
{
    using MSIL;

    internal sealed class HashedWithILEmit
    {
        public int Number { get; set; }

        public string Text { get; set; }

        public override int GetHashCode() => FastEasyHash<HashedWithILEmit>.GetHashCode(this);
        public override bool Equals(object obj) => FastEasyHash<HashedWithILEmit>.Equals(this, obj);
    }
}
