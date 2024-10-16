using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ChunkyMonkey.CodeGenerator.Extensions
{
    public static class RoslynExtensions
    {
        /// <summary>
        /// Gets the namespace that contains the given ClassDeclarationSyntax.
        /// </summary>
        /// <param name="classDeclaration">The class declaration node.</param>
        /// <returns>The namespace as a string, or null if no namespace is found.</returns>
        public static string? GetNamespace(this ClassDeclarationSyntax classDeclaration)
        {
            // Traverse the parent nodes to find the NamespaceDeclarationSyntax
            var namespaceDeclaration = classDeclaration
                .Ancestors()
                .OfType<NamespaceDeclarationSyntax>()
                .FirstOrDefault();

            if (namespaceDeclaration == null)
            {
                return null; // No namespace found (e.g., global namespace)
            }

            // Return the fully qualified namespace
            return namespaceDeclaration.Name.ToString();
        }

        /// <summary>
        /// Gets a value indicating whether the given ClassDeclarationSyntax is sealed.
        /// </summary>
        /// <param name="classDeclaration">The class declaration node.</param>
        /// <returns>True if the class is sealed, otherwise false.</returns>

        public static bool IsSealed(this ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.Modifiers.Any(SyntaxKind.SealedKeyword);
        }
    }
}

