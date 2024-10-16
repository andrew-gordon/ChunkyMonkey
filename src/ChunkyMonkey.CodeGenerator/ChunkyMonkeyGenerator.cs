using ChunkyMonkey.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace ChunkyMonkey.CodeGenerator
{
    [Generator(LanguageNames.CSharp)]
    public class ChunkyMonkeyGenerator : IIncrementalGenerator
    {
        /// <summary>
        /// Initializes the ChunkyMonkeyGenerator.
        /// </summary>
        /// <param name="context">The IncrementalGeneratorInitializationContext.</param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // System.Diagnostics.Debugger.Launch();

            // Collect class declarations
            var syntaxProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is ClassDeclarationSyntax,
                    transform: static (ctx, _) =>
                    {
                        var classDeclaration = (ClassDeclarationSyntax)ctx.Node;

                        // Check if the class has the ChunkAttribute attribute
                        var hasChunkAttribute = classDeclaration.AttributeLists
                            .SelectMany(attrList => attrList.Attributes)
                            .Any(attr => attr.Name.ToString() == nameof(ChunkAttribute) || attr.Name.ToString() == nameof(ChunkAttribute).Replace("Attribute", ""));

                        return hasChunkAttribute ? classDeclaration : null;
                    })
                .Where(static m => m != null);


            // Register the CompilationProvider to access the Compilation object
            IncrementalValueProvider<Compilation> compilationProvider = context.CompilationProvider;

            string generatedCode = string.Empty;

            // Register the output generation
            context.RegisterSourceOutput(syntaxProvider, (spc, classDecl) =>
            {
                if (classDecl != null)
                {
                    // Here you can generate code based on the class declaration
                    var className = classDecl?.Identifier.Text;
                    generatedCode = GenerateChunkingCode(className!, classDecl!);

                    spc.AddSource($"{className}_Chunk.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
                }
            });

            // Use the Compilation object to get the C# language version
            context.RegisterSourceOutput(compilationProvider, (sourceProductionContext, compilation) =>
            {
                // Get the language version from the Compilation object
                if (compilation is CSharpCompilation csharpCompilation)
                {
                    var languageVersion = csharpCompilation.LanguageVersion;
                }
            });
        }

        private string GenerateChunkingCode(string className, ClassDeclarationSyntax classDeclaration)
        {
            var namespaceText = classDeclaration.GetNamespace();

            namespaceText ??= "Unknown_Namespace";

            var typeRules = new List<TypeRule>();

            var chunkCodeFactory = new ChunkCodeFactory();
            var MergeChunksCodeFactory = new MergeChunksCodeFactory();

            typeRules.AddRange(
                [
                    new TypeRule(
                                    name: "List",
                                    typeMatcher: propertyName => propertyName.StartsWith("List<"),
                                    lengthPropertyName: "Count",
                                    chunkCodeFactory: chunkCodeFactory.ForListProperty,
                                    MergeChunksCodeFactory: MergeChunksCodeFactory.ForListProperty,
                                    newInstance: pi => $"new {pi.TypeName}()"),
                                new TypeRule(
                                    name: "Collection",
                                    typeMatcher: propertyType => propertyType.StartsWith("Collection<"),
                                    lengthPropertyName: "Count",
                                    chunkCodeFactory: chunkCodeFactory.ForCollectionProperty,
                                    MergeChunksCodeFactory: MergeChunksCodeFactory.ForCollectionProperty,
                                    newInstance: pi => $"new {pi.TypeName}()"),
                                new TypeRule(
                                    name: "Dictionary",
                                    typeMatcher: propertyName => propertyName.StartsWith("Dictionary<"),
                                    lengthPropertyName: "Count",
                                    chunkCodeFactory: chunkCodeFactory.ForDictionaryProperty,
                                    MergeChunksCodeFactory: MergeChunksCodeFactory.ForDictionaryProperty,
                                    newInstance: pi => $"new {pi.TypeName}()"),
                                new TypeRule(
                                    name: "Array",
                                    typeMatcher: propertyName => propertyName.EndsWith("[]"),
                                    lengthPropertyName: "Length",
                                    chunkCodeFactory: chunkCodeFactory.ForArrayProperty,
                                    MergeChunksCodeFactory: MergeChunksCodeFactory.ForArrayProperty,
                                    newInstance: pi => $"Array.Empty<{pi.ArrayElementType}>()"),
                                //new TypeRule(
                                //    name: "Array",
                                //    typeMatcher: propertyName => propertyName.EndsWith("[]?"),
                                //    lengthPropertyName: "Length",
                                //    chunkCodeFactory: chunkCodeFactory.ForNullableArrayProperty,
                                //    MergeChunksCodeFactory: MergeChunksCodeFactory.ForNullableArrayProperty,
                                //    newInstance: pi => $"Array.Empty<{pi.ArrayElementType}?>()"),
                            ]
             );

            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Collections.ObjectModel;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("");

            sb.AppendLine($"namespace {namespaceText}");
            sb.AppendLine("{");

            // use same namespace as the original class

            sb.AppendLine($"    public partial class {className}");
            sb.AppendLine("    {");

            sb.AppendLine("        /// <summary>");
            sb.AppendLine("        /// Chunks the instance into multiple instances based on the specified chunk size.");
            sb.AppendLine("        /// </summary>");
            sb.AppendLine("        /// <param name=\"chunkSize\">The size of each chunk.</param>");
            sb.AppendLine("        /// <returns>An enumerable of chunked instances.</returns>");
            sb.AppendLine("        public IEnumerable<" + className + "> Chunk(int chunkSize)");
            sb.AppendLine("        {");

            sb.AppendLine($"            int maxCollectionLength = 0;");

            // Loop through properties and generate chunking logic for collection properties
            foreach (var member in classDeclaration.Members)
            {
                if (member is PropertyDeclarationSyntax property)
                {
                    var propertyType = property.Type.ToString();

                    var typeRule = typeRules.FirstOrDefault(x => x.TypeMatcher(propertyType));
                    if (typeRule != null)
                    {
                        var propertyName = property.Identifier.Text;

                        sb.AppendLine($"            if (this.{propertyName}.{typeRule.LengthPropertyName} > maxCollectionLength)");
                        sb.AppendLine($"            {{");
                        sb.AppendLine($"                maxCollectionLength = this.{propertyName}.{typeRule.LengthPropertyName};");
                        sb.AppendLine($"            }}");
                    }
                }
            }

            sb.AppendLine($"");
            sb.AppendLine($"            for (int i = 0; i < maxCollectionLength; i += chunkSize)");
            sb.AppendLine($"            {{");
            sb.AppendLine($"                var instance = new {className}();");



            // Loop through properties and generate chunking logic for collection properties
            foreach (var member in classDeclaration.Members)
            {
                if (member is PropertyDeclarationSyntax property)
                {
                    var propertyInfo = GetPropertyInfo(property);

                    var typeRule = typeRules.FirstOrDefault(x => propertyInfo.TypeName != null && x.TypeMatcher(propertyInfo.TypeName));

                    if (typeRule != null)
                    {
                        var line = typeRule.ChunkCodeFactory(propertyInfo, typeRule);
                        sb.AppendLine(line);
                    }
                    else
                    {
                        sb.AppendLine($"                instance.{propertyInfo.Name} = this.{propertyInfo.Name};");
                    }
                }
            }

            sb.AppendLine($"                yield return instance;");
            sb.AppendLine($"            }}");
            sb.AppendLine($"        }}");
            sb.AppendLine("");



            // Code to merge chunks

            sb.AppendLine($"        /// <summary>");
            sb.AppendLine($"        /// Merges the specified chunks into a single instance.");
            sb.AppendLine($"        /// </summary>");
            sb.AppendLine($"        /// <param name=\"chunks\">The chunks to merge.</param>");
            sb.AppendLine($"        /// <returns>The merged instance.</returns>");
            sb.AppendLine($"        public static {className} MergeChunks(IEnumerable<{className}> chunks)");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            var instance = new {className}();");
            sb.AppendLine("");
            sb.AppendLine($"            foreach(var chunk in chunks)");
            sb.AppendLine($"            {{");

            // Loop through properties and generate chunking logic for collection properties
            foreach (var member in classDeclaration.Members)
            {
                if (member is PropertyDeclarationSyntax property)
                {
                    var propertyInfo = GetPropertyInfo(property);

                    var typeRule = typeRules.FirstOrDefault(x => propertyInfo.TypeName != null && x.TypeMatcher(propertyInfo.TypeName));
                    if (typeRule != null)
                    {
                        if (typeRule != null)
                        {
                            var line = typeRule.MergeChunksCodeFactory(propertyInfo, typeRule);
                            sb.AppendLine(line);
                        }
                    }
                    else
                    {
                        sb.AppendLine($"                  instance.{propertyInfo.Name} = chunk.{propertyInfo.Name};");
                    }
                }
            }

            sb.AppendLine($"            }}");
            sb.AppendLine("");
            sb.AppendLine($"            return instance;");
            sb.AppendLine($"        }}");
            sb.AppendLine("   }");
            sb.AppendLine("}");

            var code = sb.ToString();            

            return code;
        }

        private PropertyInfo GetPropertyInfo(PropertyDeclarationSyntax property)
        {
            bool isArray = false;
            string? arrayElementType = null;

            if (property.Type is NullableTypeSyntax nullableTypeSyntax)
            {
                // Check if the underlying type is an array type
                if (nullableTypeSyntax.ElementType is ArrayTypeSyntax arrayType)
                {
                    isArray = true;
                    arrayElementType = arrayType.ElementType.ToString();
                }
            }
            else
            {
                var syntax = property.Type as ArrayTypeSyntax;

                if (syntax != null)
                {
                    isArray = true;
                    arrayElementType = syntax.ElementType.ToString();
                }
            }

            var p = new PropertyInfo
            {
                Name = property.Identifier.Text,
                TypeName = property.Type.ToString(),
                IsArray = isArray,
                ArrayElementType = arrayElementType
            };

            return p;
        }
    }
}