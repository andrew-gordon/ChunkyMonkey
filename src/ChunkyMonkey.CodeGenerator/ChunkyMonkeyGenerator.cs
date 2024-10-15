using ChunkyMonkey.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace ChunkyMonkey.CodeGenerator
{
    [Generator(LanguageNames.CSharp)]
    public class ChunkyMonkeyGenerator : IIncrementalGenerator
    {
        // Check Chunk method doesn't already exist
        // Check MergeChunks method doesn't already exist

        // Check that the existing class is a partial class (if not, compiler warning)
        // Check that the existing class is not sealed (if so, compiler warning)
        // Check that the existing class is not static (if so, compiler warning)
        // Check that the existing class is not abstract (if so, compiler warning)
        // Check that the existing class is not a struct (if so, compiler warning)

        // Check that the existing class has a parameterless constructor (if not, compiler warning)
        // Check that the existing class has a public constructor (if not, compiler warning)

        // Handle nullable reference types

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            //System.Diagnostics.Debugger.Launch();

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
                .Where(static m => m is not null);


            // Register the output generation
            context.RegisterSourceOutput(syntaxProvider, (spc, classDecl) =>
            {
                if (classDecl is not null)
                {
                    // Here you can generate code based on the class declaration
                    var className = classDecl?.Identifier.Text;
                    var generatedCode = GenerateChunkingCode(className!, classDecl!);

                    spc.AddSource($"{className}_Chunked.g.cs", SourceText.From(generatedCode, Encoding.UTF8));
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
                ]
             );

            var sb = new StringBuilder();

            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Collections.ObjectModel;");
            sb.AppendLine("");

            sb.AppendLine($"namespace {namespaceText}");
            sb.AppendLine("{");

            // use same namespace as the original class

            sb.AppendLine($"    public partial class {className}");
            sb.AppendLine("    {");

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
                    if (typeRule is not null)
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

                    var typeRule = typeRules.FirstOrDefault(x => propertyInfo.TypeName is not null && x.TypeMatcher(propertyInfo.TypeName));

                    if (typeRule is not null)
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

                    var typeRule = typeRules.FirstOrDefault(x => propertyInfo.TypeName is not null && x.TypeMatcher(propertyInfo.TypeName));
                    if (typeRule is not null)
                    {
                        if (typeRule is not null)
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


            return sb.ToString();
        }

        private PropertyInfo GetPropertyInfo(PropertyDeclarationSyntax property)
        {
            var syntax = property.Type as ArrayTypeSyntax;
            bool isArray = false;

            string? arrayElementType = null;

            if (syntax is not null)
            {
                isArray = true;
                arrayElementType = syntax.ElementType.ToString();
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

        internal class ChunkCodeFactory
        {
            internal string ForArrayProperty(PropertyInfo propertyInfo, TypeRule _)
            {
                //var newInstanceCommand = typeRule.NewInstance(propertyInfo);

                var sb = new StringBuilder();
                sb.AppendLine($"                {{");
                sb.AppendLine($"                    if (this.{propertyInfo.Name} is not null)");
                sb.AppendLine($"                    {{");
                //sb.AppendLine($"                        if (instance.{propertyInfo.Name} is null)");
                //sb.AppendLine($"                        {{");
                //sb.AppendLine($"                            instance.{propertyInfo.Name} = {newInstanceCommand};");
                //sb.AppendLine($"                        }}");

                sb.AppendLine($"                        instance.{propertyInfo.Name} = this.{propertyInfo.Name}.Skip(i).Take(chunkSize).ToArray();");
                sb.AppendLine($"                    }}");
                sb.AppendLine($"                }}");
                sb.AppendLine($"");
                return sb.ToString();
            }

#pragma warning disable IDE0060 // Remove unused parameter
            internal string ForDictionaryProperty(PropertyInfo propertyInfo, TypeRule typeRule)
#pragma warning restore IDE0060 // Remove unused parameter
            {
                var sb = new StringBuilder();
                sb.AppendLine($"                {{");
                sb.AppendLine($"                    var dict = new {propertyInfo.TypeName}();");
                sb.AppendLine($"");
                sb.AppendLine($"                    if (this.{propertyInfo.Name} is not null)");
                sb.AppendLine($"                    {{");
                sb.AppendLine($"                        var chunkPairs = this.{propertyInfo.Name}.Skip(i).Take(chunkSize);");
                sb.AppendLine($"                        foreach(var kvp in chunkPairs)");
                sb.AppendLine($"                        {{");
                sb.AppendLine($"                            dict.Add(kvp.Key, kvp.Value);");
                sb.AppendLine($"                        }}");
                sb.AppendLine($"                        instance.{propertyInfo.Name} = dict;");
                sb.AppendLine($"                    }}");
                sb.AppendLine($"                }}");
                sb.AppendLine($"");
                return sb.ToString();
            }

#pragma warning disable IDE0060 // Remove unused parameter
            internal string ForCollectionProperty(PropertyInfo propertyInfo, TypeRule typeRule)
#pragma warning restore IDE0060 // Remove unused parameter
            {
                return $"                instance.{propertyInfo.Name} = new {propertyInfo.TypeName}(this.{propertyInfo.Name}.Skip(i).Take(chunkSize).ToList());";
            }

#pragma warning disable IDE0060 // Remove unused parameter
            internal string ForListProperty(PropertyInfo propertyInfo, TypeRule typeRule)
#pragma warning restore IDE0060 // Remove unused parameter
            {
                return $"                instance.{propertyInfo.Name} = this.{propertyInfo.Name}.Skip(i).Take(chunkSize).ToList();";
            }
        }

        internal class MergeChunksCodeFactory
        {
            internal string ForArrayProperty(PropertyInfo propertyInfo, TypeRule typeRule)
            {
                var newInstanceCommand = typeRule.NewInstance(propertyInfo); // .Replace("{typeName}", propertyInfo.TypeName).Replace("[][0]", "[0]");


                var sb = new StringBuilder();
                sb.AppendLine($"");
                sb.AppendLine($"                if (chunk.{propertyInfo.Name} is not null)");
                sb.AppendLine($"                {{");
                sb.AppendLine($"                    if (instance.{propertyInfo.Name} is null)");
                sb.AppendLine($"                    {{");
                sb.AppendLine($"                        instance.{propertyInfo.Name} = {newInstanceCommand};");
                sb.AppendLine($"                    }}");
                sb.AppendLine($"");
                sb.AppendLine($"                    instance.{propertyInfo.Name} = instance.{propertyInfo.Name}.Concat(chunk.{propertyInfo.Name}).ToArray();");
                sb.AppendLine($"                }}");
                return sb.ToString();
            }

#pragma warning disable IDE0060 // Remove unused parameter
            internal string ForDictionaryProperty(PropertyInfo propertyInfo, TypeRule typeRule)
#pragma warning restore IDE0060 // Remove unused parameter
            {
                var sb = new StringBuilder();
                sb.AppendLine($"");
                sb.AppendLine($"                if (chunk.{propertyInfo.Name} is not null)");
                sb.AppendLine($"                {{");
                sb.AppendLine($"                    if (instance.{propertyInfo.Name} is null)");
                sb.AppendLine($"                    {{");
                sb.AppendLine($"                        instance.{propertyInfo.Name} = new {propertyInfo.TypeName}();");
                sb.AppendLine($"                    }}");
                sb.AppendLine($"");
                sb.AppendLine($"                    foreach(var kvp in chunk.{propertyInfo.Name})");
                sb.AppendLine($"                    {{");
                sb.AppendLine($"                        instance.{propertyInfo.Name}.Add( kvp.Key, kvp.Value);");
                sb.AppendLine($"                    }}");
                sb.AppendLine($"                }}");
                return sb.ToString();
            }

#pragma warning disable IDE0060 // Remove unused parameter
            internal string ForCollectionProperty(PropertyInfo propertyInfo, TypeRule typeRule)
#pragma warning restore IDE0060 // Remove unused parameter
            {
                var sb = new StringBuilder();
                sb.AppendLine($"");
                sb.AppendLine($"                if (chunk.{propertyInfo.Name} is not null)");
                sb.AppendLine($"                {{");
                sb.AppendLine($"                    if (instance.{propertyInfo.Name} is null)");
                sb.AppendLine($"                    {{");
                sb.AppendLine($"                        instance.{propertyInfo.Name} = new {propertyInfo.TypeName}();");
                sb.AppendLine($"                    }}");
                sb.AppendLine($"");
                sb.AppendLine($"                    if (chunk.{propertyInfo.Name} is not null)");
                sb.AppendLine($"                    {{");
                sb.AppendLine($"                        foreach(var value in chunk.{propertyInfo.Name})");
                sb.AppendLine($"                        {{");
                sb.AppendLine($"                            instance.{propertyInfo.Name}.Add(value);");
                sb.AppendLine($"                        }}");
                sb.AppendLine($"                    }}");
                sb.AppendLine($"                }}");
                return sb.ToString();
            }

#pragma warning disable IDE0060 // Remove unused parameter
            internal string ForListProperty(PropertyInfo propertyInfo, TypeRule typeRule)
#pragma warning restore IDE0060 // Remove unused parameter
            {
                var sb = new StringBuilder();
                sb.AppendLine($"");
                sb.AppendLine($"                if (chunk.{propertyInfo.Name} is not null)");
                sb.AppendLine($"                {{");
                sb.AppendLine($"                    if (instance.{propertyInfo.Name} is null)");
                sb.AppendLine($"                    {{");
                sb.AppendLine($"                        instance.{propertyInfo.Name} = new {propertyInfo.TypeName}();");
                sb.AppendLine($"                    }}");
                sb.AppendLine($"");
                sb.AppendLine($"                    if (chunk.{propertyInfo.Name} is not null)");
                sb.AppendLine($"                    {{");
                sb.AppendLine($"                        foreach(var value in chunk.{propertyInfo.Name})");
                sb.AppendLine($"                        {{");
                sb.AppendLine($"                            instance.{propertyInfo.Name}.Add(value);");
                sb.AppendLine($"                        }}");
                sb.AppendLine($"                    }}");
                sb.AppendLine($"                }}");
                return sb.ToString();
            }
        }

        internal class TypeRule(
            string name, Func<string, bool> typeMatcher,
            string lengthPropertyName,
            Func<PropertyInfo, TypeRule, string> chunkCodeFactory,
            Func<PropertyInfo, TypeRule, string> MergeChunksCodeFactory,
            Func<PropertyInfo, string> newInstance)
        {
            public string Name { get; } = name;
            public Func<string, bool> TypeMatcher { get; } = typeMatcher;
            public string LengthPropertyName { get; } = lengthPropertyName;
            public Func<PropertyInfo, TypeRule, string> ChunkCodeFactory { get; } = chunkCodeFactory;
            public Func<PropertyInfo, TypeRule, string> MergeChunksCodeFactory { get; } = MergeChunksCodeFactory;
            public Func<PropertyInfo, string> NewInstance { get; } = newInstance;
        }

        internal class PropertyInfo
        {
            public string? Name { get; set; }
            public string? TypeName { get; set; }
            public bool IsArray { get; set; }
            public string? ArrayElementType { get; set; }
        }
    }
}