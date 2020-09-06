using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using MyAnalyzer;

namespace MyAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ProxyClassMustPostFixWithProxyAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "Proxy";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "Proxy", "Proxy class {0} must postfix with proxy word", "Naming", DiagnosticSeverity.Warning, true, "");

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            try
            {
                var classDeclaration = (ClassDeclarationSyntax)context.Node;
                var isProxyClass = classDeclaration.IsProxyClass();

                if (isProxyClass)
                {
                    var isNamEndsWithProxy = classDeclaration.Identifier.ValueText.EndsWith("Proxy");
                    if (!isNamEndsWithProxy)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation(), classDeclaration.Identifier.ValueText));
                    }
                }

            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }
}
