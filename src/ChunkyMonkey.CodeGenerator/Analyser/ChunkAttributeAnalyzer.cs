using System.Collections.Immutable;
using ChunkyMonkey.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ChunkyMonkey.CodeGenerator.Analyser
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ChunkAttributeAnalyzer : DiagnosticAnalyzer
    {
        private const string DiagnosticId = "MY001";
        private static readonly DiagnosticDescriptor Rule = new(
            DiagnosticId,
            "Invalid use of MyCustomAttribute",
            "ChunkAttribute can only be applied to unsealed, non-abstract, non-static classes",
            "Usage",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            // Register a syntax node action for class declarations
            context.RegisterSyntaxNodeAction(AnalyzeClass, SyntaxKind.ClassDeclaration);
        }

        private void AnalyzeClass(SyntaxNodeAnalysisContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            // Get the class's symbol
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

            if (classSymbol == null)
            {
                return;
            }

            // Check if the class has the ChunkAttribute
            var hasCustomAttribute = classSymbol.GetAttributes().Any(attr =>
                attr.AttributeClass?.Name == nameof(ChunkAttribute) || attr.AttributeClass?.Name == nameof(ChunkAttribute).Replace("Attribute", ""));

            if (!hasCustomAttribute)
            {
                return;
            }

            // Check if the class is sealed, abstract, or static
            bool isSealed = classSymbol.IsSealed;
            bool isAbstract = classSymbol.IsAbstract;
            bool isStatic = classSymbol.IsStatic;

            if (isSealed || isAbstract || isStatic)
            {
                // Report a diagnostic if the class violates the rules
                var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }

            // Check if the class has a parameterless constructor
            bool hasParameterlessConstructor = classSymbol.Constructors
                .Any(constructor => constructor.Parameters.IsEmpty && !constructor.IsStatic);

            // If the class does not have a parameterless constructor, report a diagnostic
            if (!hasParameterlessConstructor)
            {
                var diagnostic = Diagnostic.Create(Rule, classDeclaration.Identifier.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

}
