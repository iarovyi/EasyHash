namespace EasyHash.Specs
{
    using System;
    using FluentAssertions;
    using Targets;
    using Xunit;

    public class EasyHashSpecs
    {
        [Fact]
        public void Should_generate_the_same_code_when_have_the_same_data()
        {
            EasyHash<Foo>.Reset();
            EasyHash<Foo>.Register(r => { });
            var one = new Foo() { Text = "some text" };
            var two = one.Clone();

            one.GetHashCode().Should().Be(two.GetHashCode());
        }

        [Fact]
        public void Should_generate_different_code_when_have_different_data()
        {
            EasyHash<Foo>.Reset();
            EasyHash<Foo>.Register(r => { });
            var one = new Foo() { Text = "some text" };
            var two = new Foo() { Text = "another text" };

            one.GetHashCode().Should().NotBe(two.GetHashCode());
        }

        [Fact]
        public void Should_not_allow_register_type_twice()
        {
            EasyHash<Foo>.Reset();

            EasyHash<Foo>.Register(r => { });
            EasyHash<Foo>.Register(r => { });

            Action action = () => new Foo().GetHashCode();
            action.ShouldThrow<InvalidOperationException>();
        }

        [Fact]
        public void Should_use_default_hashing_if_have_no_registration()
        {
            EasyHash<Foo>.Reset();

            new Foo().GetHashCode().Should().NotBe(0);
        }

        [Fact]
        public void Should_use_custom_hashing_if_registered()
        {
            var foo = new Foo();
            EasyHash<Foo>.Reset();
            int defaultHashCode = foo.GetHashCode();
            EasyHash<Foo>.Register(r => r.WithPrimes(23,31));

            int customHashCode = foo.GetHashCode();

            defaultHashCode.Should().NotBe(customHashCode);
        }

        [Fact]
        public void Should_not_throw_during_registration()
        {
            Action action = ()=> EasyHash<Foo>.Register(r => { throw new Exception(); });
            action.ShouldNotThrow();
        }

        [Fact]
        public void Should_throw_initialization_exception_during_hashing()
        {
            EasyHash<Foo>.Register(r => { throw new ApplicationException(); });

            Action action = () => new Foo().GetHashCode();
            action.ShouldThrow<ApplicationException>();
        }
    }
}
