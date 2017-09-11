namespace EasyHash.Benchmark.Benchmarks.Targets
{
    internal sealed class HashedManually
    {
        public int Number { get; set; }

        public string Text { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is HashedManually && GetHashCode() == obj.GetHashCode();
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hash = Number;
                hash = (hash * 16777619) ^ (Text == null ? 0 : Text.GetHashCode());
                return hash;
            }
        }
    }
}
