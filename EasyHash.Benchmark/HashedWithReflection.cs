namespace EasyHash.Benchmark
{
    using Helpers;

    internal sealed class HashedWithReflection
    {
        public int Number { get; set; }

        public string Text { get; set; }

        public override int GetHashCode() => ReflectionHasher<HashedWithReflection>.GetHashCode(this);
        public override bool Equals(object obj) => ReflectionHasher<HashedWithReflection>.Equals(this, obj);
    }
}
