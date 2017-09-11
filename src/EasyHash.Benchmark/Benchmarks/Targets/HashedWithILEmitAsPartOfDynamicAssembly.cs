namespace EasyHash.Benchmark.Benchmarks.Targets
{
    using MSIL;

    public sealed class HashedWithILEmitAsPartOfDynamicAssembly
    {
        public int Number { get; set; }

        public string Text { get; set; }

        public override int GetHashCode() => EasyHashUsingDynamicAssembly<HashedWithILEmitAsPartOfDynamicAssembly>.GetHashCode(this);
        public override bool Equals(object obj) => EasyHashUsingDynamicAssembly<HashedWithILEmitAsPartOfDynamicAssembly>.Equals(this, obj);
    }
}

