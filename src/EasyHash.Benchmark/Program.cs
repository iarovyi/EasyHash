namespace EasyHash.Benchmark
{
    using System;
    using static System.Console;
    using static Helpers.ConsoleHelper;
    using Benchmarks;

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //GetHashCodeBenchmark.Run();
                GetHashCodeCustomBenchmark.Run();
            }
            catch (Exception ex)
            {
                WriteLine(ex.Message, ConsoleColor.Red);
            }

            WriteLine("Press enter to exit", ConsoleColor.Yellow);
            ReadKey();
        }
    }
}
