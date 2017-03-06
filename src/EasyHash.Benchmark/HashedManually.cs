namespace EasyHash.Benchmark
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
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ (Number == 0 ? 0 : Number.GetHashCode());
                hash = (hash * 16777619) ^ (Text == null ? 0 : Text.GetHashCode());
                return hash;
            }
        }
    }
}
