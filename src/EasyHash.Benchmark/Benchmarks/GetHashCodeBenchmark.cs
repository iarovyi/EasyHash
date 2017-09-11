namespace EasyHash.Benchmark.Benchmarks
{
    /*using BenchmarkDotNet.Attributes;
    using BenchmarkDotNet.Running;
    using FizzWare.NBuilder;
    using Targets;

    public class GetHashCodeBenchmark
    {
        private readonly HashedManually _hashedManually = New<HashedManually>();
        private readonly HashedWithEasyHash _hashedWithEasyHash = New<HashedWithEasyHash>();
        private readonly HashedWithILEmit _hashedWithIlEmit = New<HashedWithILEmit>();
        private readonly HashedWithILEmitAsPartOfDynamicAssembly _hashedWithIlEmitAsPartOfDynamicAssembly = New<HashedWithILEmitAsPartOfDynamicAssembly>();
        private readonly HashedWithReflection _hashedWithReflection = New<HashedWithReflection>();

        [Benchmark] public int GetHashCodeOfHashedManually() => _hashedManually.GetHashCode();
        [Benchmark] public int GetHashCodeOfHashedWithEasyHash() => _hashedWithEasyHash.GetHashCode();
        [Benchmark] public int GetHashCodeOfHashedWithIlEmit() => _hashedWithIlEmit.GetHashCode();
        [Benchmark] public int GetHashCodeOfHashedWithIlEmitAsPartOfDynamicAssembly() => _hashedWithIlEmitAsPartOfDynamicAssembly.GetHashCode();
        [Benchmark] public int GetHashCodeOfHashedWithReflection() => _hashedWithReflection.GetHashCode();

        public static void Run() => BenchmarkRunner.Run<GetHashCodeBenchmark>();

        private static T New<T>() => Builder<T>.CreateNew().Build();
    }*/
}
