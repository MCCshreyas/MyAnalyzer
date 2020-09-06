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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AddAntiForgeryTokenCodeFixProvider)), Shared]
    public class AddAntiForgeryTokenCodeFixProvider : CodeFixProvider
    {
        private const string title = "Add AntiForgeryToken";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(HttpPostMethodMustHaveValidateAntiForgeryTokenAttributeAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => RenameWithProxyPostFix(context.Document, diagnostic, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> RenameWithProxyPostFix(Document document,
            Diagnostic diagnostic,
            MethodDeclarationSyntax methodDeclaration,
            CancellationToken cancellationToken)
        {

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var attributes = methodDeclaration.AttributeLists.Add(
                SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("ValidateAntiForgeryToken"))
                )));

            return document.WithSyntaxRoot(
                root.ReplaceNode(
                    methodDeclaration,
                    methodDeclaration.WithAttributeLists(attributes)
                ));
        }
    }
}