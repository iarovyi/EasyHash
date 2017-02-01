namespace EasyHash.Specs
{
    using System;
    using FluentAssertions;
    using Helpers.Extensions;
    using Targets;
    using Xunit;
    using Moq;

    public sealed class GetHashCodeExpressionBuilderSpecs
    {
        [Fact]
        public void Should_succeed_with_overflow()
        {
            checked
            {
                var foo = new Foo() {Number = int.MaxValue, Text = "some text"};
                Func<Foo,int> hashFn = new GetHashCodeExpressionBuilder<Foo>()
                    .WithPrimes(int.MaxValue, int.MaxValue)
                    .Build()
                    .Compile();

                Action action = () => hashFn(foo);
                action.ShouldNotThrow();
            }
        }

        [Fact]
        public void Should_not_take_member_into_account_if_it_is_registered_as_skipped()
        {
            var foo = new Foo();
            Func<Foo, int> skippedFn = new GetHashCodeExpressionBuilder<Foo>()
                .Skip(f => f.Number)
                .Build()
                .Compile();

            int skippedNumberHash = skippedFn(foo);

            skippedNumberHash.Should().BeNotEqualToDefaultHashCodeOf(foo);
        }

        [Fact]
        public void Should_use_provided_primes()
        {
            var foo = new Foo();
            Func<Foo, int> primesFn = new GetHashCodeExpressionBuilder<Foo>()
                .WithPrimes(31,41)
                .Build()
                .Compile();

            int customPrimeHash = primesFn(foo);

            customPrimeHash.Should().BeNotEqualToDefaultHashCodeOf(foo);
        }

        [Fact]
        public void Should_use_member_hashing_function()
        {
            var foo = new Foo();
            Func<Foo, int> customFn = new GetHashCodeExpressionBuilder<Foo>()
                .For(f => f.Number, (h, i) => 777)
                .Build()
                .Compile();

            int customNumberHash = customFn(foo);

            customNumberHash.Should().BeNotEqualToDefaultHashCodeOf(foo);
        }

        [Fact]
        public void Should_hash_collection_items()
        {
            var item = new Mock<Foo>();
            var foo = new Foo() { Foos = new [] { item.Object, item.Object } };
            Func<Foo, int> hashFn = new GetHashCodeExpressionBuilder<Foo>().Build().Compile();

            hashFn(foo);

            item.Verify(i => i.GetHashCode(), Times.Exactly(foo.Foos.Length));
        }

        [Fact]
        public void Should_not_hash_collection_items_if_registered()
        {
            var item = new Mock<Foo>();
            var foo = new Foo() { Foos = new[] { item.Object, item.Object } };
            Func<Foo, int> hashFn = new GetHashCodeExpressionBuilder<Foo>()
                .ExcludeCollectionItems()
                .Build()
                .Compile();

            hashFn(foo);

            item.Verify(i => i.GetHashCode(), Times.Exactly(0));
        }

        [Fact]
        public void Should_take_into_account_order_of_items_in_collection_property()
        {
            var fooOrdered = new Foo() { Words = new[]{ "a", "b" } };
            var fooReversed = new Foo() { Words = new[]{ "b", "a" } };
            Func<Foo, int> hashFn = new GetHashCodeExpressionBuilder<Foo>().Build().Compile();

            int orderedColHash = hashFn(fooOrdered);
            int reversedColHash = hashFn(fooReversed);

            orderedColHash.Should().NotBe(reversedColHash);
        }
    }
}
