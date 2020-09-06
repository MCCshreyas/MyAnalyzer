using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MyAnalyzer
{
    public static class RoslynExtensions
    {
        public static bool IsBaseController(this ClassDeclarationSyntax classDeclaration)
        {
            if (classDeclaration is null)
                return false; 
            
            var isBaseControllerType = classDeclaration.BaseList != null && classDeclaration.BaseList.Types.Select(x => (GenericNameSyntax)x.Type).Select(x => x.Identifier.ValueText == "BaseController").FirstOrDefault();

            return isBaseControllerType;
        }

        public static bool IsProxyClass(this ClassDeclarationSyntax classDeclaration)
        {
            if (classDeclaration is null)
                return false;

            return classDeclaration.DoesInheritFromGenericBaseClass("WcfProxyBase");
        }

        public static bool DoesInheritFromGenericBaseClass(this ClassDeclarationSyntax classDeclaration, string genericClassName)
        {
            if (classDeclaration is null)
                return false; 
            
            var isProxyClass = classDeclaration.BaseList != null && classDeclaration.BaseList.Types.Select(x => (GenericNameSyntax)x.Type).Select(x => x.Identifier.ValueText == genericClassName).FirstOrDefault();

            return isProxyClass;
        }

        public static bool HasMethodAttribute(this MethodDeclarationSyntax methodDeclaration, string attributeName)
        {
            return methodDeclaration.AttributeLists
                .SelectMany(x => x.Attributes.Select((c => (IdentifierNameSyntax) c.Name))).Select(x => x.Identifier.ValueText).ToList().Contains(attributeName);
        }

        public static bool HasClassAttribute(this ClassDeclarationSyntax methodDeclaration, string attributeName)
        {
            return methodDeclaration.AttributeLists
                .SelectMany(x => x.Attributes.Select((c => (IdentifierNameSyntax) c.Name))).Select(x => x.Identifier.ValueText).ToList().Contains(attributeName);
        }
    }
}