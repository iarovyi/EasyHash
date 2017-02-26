namespace EasyHash.Analyzer.Specs
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using TestHelper;
    using Xunit;

    public class RegistrationAnalyzerSpecs : DiagnosticVerifier
    {
        [Fact]
        public void Should_Not_Complain_Without_Reason()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [Fact]
        public void Should_Report_Error_When_Registered_Inside_Instance_Constructor()
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
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 10, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [Fact]
        public void Should_Report_Error_When_Registered_Inside_Instance_Constructor_With_Full_Name()
        {
            var test = @"
            namespace EasyHash.Benchmark
            {
                internal class SomeType
                {
                    public SomeType()
                    {
                        EasyHash.EasyHash<SomeType>
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
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", 8, 25) }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new RegistrationAnalyzer();
    }
}