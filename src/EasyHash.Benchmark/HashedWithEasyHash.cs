namespace EasyHash.Benchmark
{
    internal sealed class HashedWithEasyHash
    {
        public int Number { get; set; }

        public string Text { get; set; }

        public override int GetHashCode() => EasyHash<HashedWithEasyHash>.GetHashCode(this);
        public override bool Equals(object obj) => EasyHash<HashedWithEasyHash>.Equals(this, obj);
    }
}
