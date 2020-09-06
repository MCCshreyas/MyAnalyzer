using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MyAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ProxyClassMustPostFixWithProxyAnalyzer)), Shared]
    public class ProxyClassMustPostFixWithProxyCodeFixProvider : CodeFixProvider
    {
        private const string title = "Add Proxy postfix";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ProxyClassMustPostFixWithProxyAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<ClassDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => RenameWithProxyPostFix(context.Document, diagnostic, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> RenameWithProxyPostFix(Document document,
            Diagnostic diagnostic,
            ClassDeclarationSyntax localDeclaration,
            CancellationToken cancellationToken)
        {

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var baseName = $"{token.ValueText}Proxy";

            var leading = localDeclaration.Identifier.LeadingTrivia;
            var trailing = localDeclaration.Identifier.TrailingTrivia;

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
            var declaredSymbol = semanticModel.GetDeclaredSymbol(token.Parent, cancellationToken);

            ClassDeclarationSyntax newClass =
                localDeclaration.WithIdentifier(SyntaxFactory.Identifier(leading, baseName, trailing));

            var newRoot = root.ReplaceNode(localDeclaration, newClass);

            return document.WithSyntaxRoot(newRoot);
        }
    }
}