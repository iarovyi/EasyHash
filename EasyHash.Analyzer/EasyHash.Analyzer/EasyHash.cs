namespace EasyHash.Analyzer
{
    using Helpers.Extensions;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    internal static class EasyHash
    {
        public const string AssemblyName = "EasyHash";
        public const string TypeName = "EasyHash";
        public const string RegistrationMethodName = "Register";

        public static bool IsEasyHashRegistration(this InvocationExpressionSyntax invocation, SyntaxNodeAnalysisContext context)
            => invocation.IsStaticCallOf(context, AssemblyName, TypeName, RegistrationMethodName);
    }
}
