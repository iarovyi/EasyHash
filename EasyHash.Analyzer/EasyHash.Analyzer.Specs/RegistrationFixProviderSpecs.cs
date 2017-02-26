namespace EasyHash.Analyzer.Specs
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using TestHelper;
    using Xunit;

    public class RegistrationFixProviderSpecs : CodeFixVerifier
    {
        [Fact]
        public void Should_Move_Registration_From_Instance_Ctor_To_Static_Constructor()
        {
            var test = @"
namespace EasyHash.Benchmark
{
    using EasyHash;

    internal class SomeType
    {
        static SomeType()
        {
        }
        public SomeType()
        {
            EasyHash<SomeType>
                .Register(r => r
                    .WithPrimes(17, 23));
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = RegistrationAnalyzer.DiagnosticId,
                Message = string.Format(Resources.AnalyzerMessageFormat, "SomeType"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 13, 13) }
            };
            VerifyCSharpDiagnostic(test, expected);

            var fixedCode = @"
namespace EasyHash.Benchmark
{
    using EasyHash;

    internal class SomeType
    {
        static SomeType()
        {
            EasyHash<SomeType>
                .Register(r => r
                    .WithPrimes(17, 23));
        }
        public SomeType()
        {
        }
    }
}";
            VerifyCSharpFix(test, fixedCode);
        }

        [Fact]
        public void Should_Create_Static_Ctor_And_Move_Registration_When_It_Doesn_Not_Exist()
        {
            var test = @"
namespace EasyHash.Benchmark
{
    using EasyHash;

    internal class SomeType
    {
        public SomeType()
        {
            EasyHash<SomeType>
                .Register(r => r
                    .WithPrimes(17, 23));
        }
    }
}";

            var expected = new DiagnosticResult
            {
                Id = RegistrationAnalyzer.DiagnosticId,
                Message = string.Format(Resources.AnalyzerMessageFormat, "SomeType"),
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 13) }
            };
            VerifyCSharpDiagnostic(test, expected);

            var fixedCode = @"
namespace EasyHash.Benchmark
{
    using EasyHash;

    internal class SomeType
    {
        public SomeType()
        {
        }
        static SomeType()
        {
            EasyHash<SomeType>
                .Register(r => r
                    .WithPrimes(17, 23));
        }
    }
}";
            VerifyCSharpFix(test, fixedCode);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new RegistrationCodeFixProvider();
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new RegistrationAnalyzer();
    }
}