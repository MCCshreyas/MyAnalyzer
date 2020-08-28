using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace MyAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MyAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MyAnalyzer";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Usage";

        readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, "Controller must have ActionAuthorize attribute as part of security.", Category, DiagnosticSeverity.Warning, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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
                if (classDeclaration.BaseList != null)
                {
                    var hasActionAuthorizeAttribute = false;

                    var types = (GenericNameSyntax)classDeclaration.BaseList.Types[0].Type;
                    var controllerName = types.Identifier.ValueText;

                    if (controllerName == "BaseController")
                    {
                        var list = classDeclaration.AttributeLists.ToList();
                        foreach (var attributeName in list.SelectMany(l =>
                            l.Attributes.Select(current => (IdentifierNameSyntax)current.Name)
                                .Select(name => name.Identifier.ValueText)))
                        {
                            hasActionAuthorizeAttribute = attributeName == "ActionAuthorize";
                        }
                    }

                    if (hasActionAuthorizeAttribute == false)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation()));
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
