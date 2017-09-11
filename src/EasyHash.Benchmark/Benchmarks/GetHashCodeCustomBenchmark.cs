namespace EasyHash.Benchmark.Benchmarks
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Helpers;
    using FizzWare.NBuilder;
    using Targets;

    internal class GetHashCodeCustomBenchmark
    {
        private const int Times = 1000000;

        public static void Run()
        {
            ConsoleHelper.WriteLine($"Benchmark started", ConsoleColor.Green);
            ConsoleHelper.WriteLine($"Each hashing implementation will be executed {Times} times", ConsoleColor.Green);

            long manualMs = Benchmark(New<HashedManually>());
            Console.WriteLine($"1) Hashing with regular implementation: {manualMs}");

            long easyHashMs = Benchmark(New<HashedWithEasyHash>());
            Console.WriteLine($"2) Hashing with EasyHash (Expression trees): {easyHashMs}");

            long ilEmitMs = Benchmark(New<HashedWithILEmit>());
            Console.WriteLine($"3) Hashing with IL Emit: {ilEmitMs}");

            long ilEmitAsDynamicAssemblyMs = Benchmark(New<HashedWithILEmitAsPartOfDynamicAssembly>());
            Console.WriteLine($"4) Hashing with IL Emit as part of dynamic assembly: {ilEmitAsDynamicAssemblyMs}");

            long reflectionMs = Benchmark(New<HashedWithReflection>());
            Console.WriteLine($"5) Hashing with cached reflection: {reflectionMs}");

            ConsoleHelper.WriteLine($"Benchmark Finished", ConsoleColor.Green);
        }

        private static T New<T>() => Builder<T>.CreateNew().Build();

        private static long Benchmark(object target, int times = Times)
        {
            InitBenchmark();

            //Warm up processor cache
            for (int i = 0; i < times; i++)
            {
                target.GetHashCode();
            }

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < times; i++)
            {
                target.GetHashCode();
            }

            return sw.ElapsedMilliseconds;
        }

        private static void InitBenchmark()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            //Force thread to be executed on core #1 as result preventing thread to jump between cores.
            Process.GetCurrentProcess().ProcessorAffinity = new IntPtr(1);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
