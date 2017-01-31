namespace EasyHash.Specs.Helpers.Extensions
{
    using System;
    using FluentAssertions;
    using FluentAssertions.Numeric;

    internal static class NumericAssertationsExtensions
    {
        public static void BeNotEqualToDefaultHashCodeOf<T>(this NumericAssertions<int> assertions, T value)
        {
            Func<T, int> defaultHashFn = new GetHashCodeExpressionBuilder<T>().Build().Compile();
            assertions.Should().NotBe(defaultHashFn(value));
        }
    }
}
