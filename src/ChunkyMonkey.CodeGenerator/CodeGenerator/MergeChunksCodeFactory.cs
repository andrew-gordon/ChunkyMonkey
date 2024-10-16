using System.Text;
using static ChunkyMonkey.CodeGenerator.CodeGenerator.ChunkyMonkeyGenerator;

namespace ChunkyMonkey.CodeGenerator.CodeGenerator
{
    internal class MergeChunksCodeFactory
    {
        internal string ForArrayProperty(PropertyInfo propertyInfo, TypeRule typeRule)
        {
            var newInstanceCommand = typeRule.NewInstance(propertyInfo);

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

        internal string ForNullableArrayProperty(PropertyInfo propertyInfo, TypeRule typeRule)
        {
            var newInstanceCommand = typeRule.NewInstance(propertyInfo);

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
}
