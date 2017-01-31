namespace EasyHash.Benchmark.Helpers
{
    using System.Linq;
    using System.Reflection;

    internal static class ReflectionHasher<T>
    {
        private static readonly PropertyInfo[] Properties = typeof (T).GetProperties().Where(p => p.CanRead).ToArray();

        public static int GetHashCode(T obj)
        {
            unchecked
            {
                int hash = (int)2166136261;

                foreach (PropertyInfo prop in Properties)
                {
                    hash = (hash * 16777619) ^ (prop.GetValue(obj)?.GetHashCode() ?? 0);
                }

                return hash;
            }
        }

        public static bool Equals(T obj, T other) => EasyHash<T>.Equals(obj, other);
    }
}
