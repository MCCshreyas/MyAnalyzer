using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MyAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MyAnalyzerCodeFixProvider)), Shared]
    public class MyAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Add ActionAuthorize attribute";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(MyAnalyzerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
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
                    createChangedDocument: c => MakeConstAsync(context.Document, declaration, c),
                    equivalenceKey: title),
                            diagnostic);
        }

        private async Task<Document> MakeConstAsync(Document document,
                                                    ClassDeclarationSyntax localDeclaration,
                                                    CancellationToken cancellationToken)
        {

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var attributes = localDeclaration.AttributeLists.Add(
                SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList<AttributeSyntax>(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("ActionAuthorize"))
                )));

            return document.WithSyntaxRoot(
                root.ReplaceNode(
                    localDeclaration,
                    localDeclaration.WithAttributeLists(attributes)
                ));

            //var variable = localDeclaration;
            //string newName = variable.Identifier.ValueText;
            //if (newName.Length > 1)
            //{
            //    newName = char.ToUpper(newName[0]) + newName.Substring(1);
            //}
            //var leading = variable.Identifier.LeadingTrivia;
            //var trailing = variable.Identifier.TrailingTrivia;

            //ClassDeclarationSyntax newVariable = variable.WithIdentifier(SyntaxFactory.Identifier(leading, newName + "R", trailing));
            //var newRoot = .ReplaceNode(variable, newVariable);
            //return new[] { CodeAction.Create("Add 'i'", document.WithSyntaxRoot(newRoot)) };
        }
    }
}
