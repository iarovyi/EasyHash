namespace EasyHash.Specs
{
    using System;
    using FluentAssertions;
    using Xunit;

    using MSIL;
    using Targets;

    public sealed class DynamicMethodGetHashCodeGeneratorSpecs
    {
        [Fact]
        public void Should_be_able_to_calculate_hash_code()
        {
            var foo = Dummy.Default;
            var configuration = new GetHashCodeConfiguration<Dummy>().WithPrimes(397,0);
            var generator = new DynamicMethodGetHashCodeGenerator<Dummy>(configuration);
            Func<Dummy, int> hashFn = generator.Build();

            int hashCode = hashFn(foo);

            int expectedHashCode = foo.GetHashCode();
            hashCode.Should().Be(expectedHashCode);
        }
    }
}
