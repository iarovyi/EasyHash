namespace EasyHash.Helpers.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class TypeExtensions
    {
        public static Type GetEnumerableType(this Type type)
        {
            Type genericDefinition =
                type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                ? type
                : type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            return genericDefinition?.GetGenericArguments()[0];
        }
    }
}
