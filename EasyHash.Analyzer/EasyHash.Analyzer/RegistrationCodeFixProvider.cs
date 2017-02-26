namespace EasyHash.Analyzer
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpers.Extensions;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;
    using Microsoft.CodeAnalysis.Text;

    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RegistrationCodeFixProvider)), Shared]
    public class RegistrationCodeFixProvider : CodeFixProvider
    {
        private static readonly string title = Resources.FixAction_MoveToStaticCtor;
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RegistrationAnalyzer.DiagnosticId);
        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            Diagnostic diagnostic = context.Diagnostics.First();
            TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

            ExpressionStatementSyntax registration = root
                .FindToken(diagnosticSpan.Start)
                .Parent
                .AncestorsAndSelf()
                .OfType<ExpressionStatementSyntax>()
                .First();

            TypeDeclarationSyntax declaration = registration
                .AncestorsAndSelf()
                .OfType<TypeDeclarationSyntax>()
                .First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => MoveRegistrationToStaticCtorAsync(context.Document, declaration, registration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> MoveRegistrationToStaticCtorAsync(
            Document document,
            TypeDeclarationSyntax origDeclaration,
            ExpressionStatementSyntax registration,
            CancellationToken cancellationToken)
        {
            TypeDeclarationSyntax newDeclaration = origDeclaration
                .RemoveNode(registration, SyntaxRemoveOptions.KeepNoTrivia);
            ConstructorDeclarationSyntax staticCtor = FindStaticCtor(origDeclaration);

            if (staticCtor == null)
            {
                staticCtor = SyntaxFactory
                    .ConstructorDeclaration(origDeclaration.Identifier)
                    .WithModifiers(
                        SyntaxTokenList.Create(
                            SyntaxFactory.Token(SyntaxKind.StaticKeyword)
                            )
                    )
                    .WithBody(SyntaxFactory.Block())
                    .WithAdditionalAnnotations(Formatter.Annotation, SyntaxAnnotation.ElasticAnnotation)
                    .NormalizeWhitespace();

                newDeclaration = newDeclaration.AddMembers(staticCtor);
            }

            BlockSyntax withRegistration = staticCtor.Body
                .WithStatements(staticCtor.Body.Statements.Add(registration))
                .WithAdditionalAnnotations(Formatter.Annotation);
            newDeclaration = newDeclaration
                .ReplaceNode(FindStaticCtor(newDeclaration).Body, withRegistration);

            SyntaxNode root = await document
                .GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = root.ReplaceNode(origDeclaration, newDeclaration);
            return document.WithSyntaxRoot(newRoot);
        }

        private ConstructorDeclarationSyntax FindStaticCtor(TypeDeclarationSyntax declaration) =>
            declaration
               .DescendantNodes()
               .OfType<ConstructorDeclarationSyntax>()
               .FirstOrDefault(c => c.Modifiers.Any(SyntaxKind.StaticKeyword));
    }
}