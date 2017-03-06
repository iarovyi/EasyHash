namespace EasyHash.Helpers
{
    using System;
    using System.Threading;

    internal static class RandomProvider
    {
        private static int _seed = Environment.TickCount;
        private static readonly ThreadLocal<Random> ThreadLocalRandom = new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref _seed)));
        //Does not conflict with System.Collections.HashHelpers.primes so there is risk that it's equal to the number of buckets in Hashtable
        private static readonly int[] Primes = { 16780633, 16780651, 16780669, 16780681, 16780723, 16780727, 16780787, 16780789, 16780801, 16780817 };

        public static Random GetThreadRandom() => ThreadLocalRandom.Value;
        public static int GetPrime() => Primes[GetThreadRandom().Next(0, Primes.Length)];
    }
}
