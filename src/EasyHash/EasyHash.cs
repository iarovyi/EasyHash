namespace EasyHash
{
    using JetBrains.Annotations;
    using System;
    using System.Threading;

    public static class EasyHash<T>
    {
        private static Func<T, int> defaultHasher;
        private static Func<T, int> hasher;
        private static Exception initializationException;

        static EasyHash()
        {
            Reset();
        }

        internal static void Reset()
        {
            try
            {
                hasher = defaultHasher = new GetHashCodeExpressionBuilder<T>().Build().Compile();
                initializationException = null;
            }
            catch (Exception e)
            {
                initializationException = e;
            }
        }

        public static void Register([NotNull]Action<GetHashCodeExpressionBuilder<T>> configure)
        {
            try
            {
                var registration = new GetHashCodeExpressionBuilder<T>();
                configure(registration);
                Func<T, int> hashFn = registration.Build().Compile();

                if (Interlocked.CompareExchange(ref hasher, hashFn, defaultHasher) != defaultHasher)
                {
                    throw new InvalidOperationException($"Type '{typeof(T)}' was already registered with EasyHash");
                }
            }
            catch (Exception ex)
            {
                initializationException = ex;
            }
        }

        //TODO: autogenerate Equals as well
        public static bool Equals(T obj, object other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(obj, other)) return true;
            return other is T && obj.GetHashCode() == other.GetHashCode();
        }

        public static int GetHashCode(T obj)
        {
            if (initializationException != null) { throw initializationException; }
            if (obj == null) { throw new ArgumentNullException(nameof(obj)); }

            return hasher(obj);
        }
    }
}
