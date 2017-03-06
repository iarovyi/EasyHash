namespace EasyHash.Analyzer
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RegistrationAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "RegistrationAnalyzer";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "EasyHash";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(EasyHashRegistrationShouldNotBeInInstanceConstructor,
                                             SyntaxKind.ConstructorDeclaration);
        }

        private void EasyHashRegistrationShouldNotBeInInstanceConstructor(SyntaxNodeAnalysisContext context)
        {
            var ctorNode = (ConstructorDeclarationSyntax)context.Node;
            if (!ctorNode.Modifiers.Any(SyntaxKind.StaticKeyword))
            {
                var registrationsInsideInstanceCtor = context.Node.DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Where(i => i.IsEasyHashRegistration(context));

                foreach (InvocationExpressionSyntax registration in registrationsInsideInstanceCtor)
                {
                    string className = ctorNode.Identifier.Text;
                    context.ReportDiagnostic(Diagnostic.Create(Rule, registration.GetLocation(), className));
                }
            }
        }
    }
}