namespace EasyHash.Benchmark.Helpers
{
    using System;

    internal static class ConsoleHelper
    {
        public static void WriteLine(string message, ConsoleColor color = ConsoleColor.White)
        {
            ConsoleColor original = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = original;
        }
    }
}
