using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MyAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class HttpPostMethodMustHaveValidateAntiForgeryTokenAttributeAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ValidateAntiForgeryToken";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, "ValidateAntiForgeryToken", "HttpPost method {0} must have ValidateAntiForgeryToken", "Security", DiagnosticSeverity.Warning, true, "");

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            try
            {
                var methodType = (MethodDeclarationSyntax)context.Node;

                if (methodType.HasMethodAttribute("HttpPost"))
                {
                    if (!methodType.HasMethodAttribute("ValidateAntiForgeryToken"))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, methodType.Identifier.GetLocation(), methodType.Identifier.ValueText));
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