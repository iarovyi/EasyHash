namespace EasyHash.Analyzer.Helpers.Extensions
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class InvocationExpressionSyntaxExtensions
    {
        public static bool IsStaticCallOf(this InvocationExpressionSyntax invocation,
                                          SyntaxNodeAnalysisContext context,
                                          string assembly,
                                          string type,
                                          string method)
        {
            var methodSyntax = invocation?.Expression as MemberAccessExpressionSyntax;
            TypeInfo typeInfo = methodSyntax?.Expression != null
                ? context.SemanticModel.GetTypeInfo(methodSyntax.Expression, context.CancellationToken)
                : new TypeInfo();

            return methodSyntax?.Name?.Identifier.ToString() == method
                   && !(typeInfo.Type is IErrorTypeSymbol)
                   && typeInfo.Type?.ContainingAssembly.Identity.Name == assembly
                   && typeInfo.Type?.Name == type;
        }
    }
}
