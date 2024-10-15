using System.Text;
using static ChunkyMonkey.CodeGenerator.ChunkyMonkeyGenerator;

namespace ChunkyMonkey.CodeGenerator
{
    internal class ChunkCodeFactory
    {
        internal string ForArrayProperty(PropertyInfo propertyInfo, TypeRule _)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"                {{");
            sb.AppendLine($"                    if (this.{propertyInfo.Name} is not null)");
            sb.AppendLine($"                    {{");
            sb.AppendLine($"                        instance.{propertyInfo.Name} = this.{propertyInfo.Name}.Skip(i).Take(chunkSize).ToArray();");
            sb.AppendLine($"                    }}");
            sb.AppendLine($"                }}");
            sb.AppendLine($"");
            return sb.ToString();
        }

        internal string ForNullableArrayProperty(PropertyInfo propertyInfo, TypeRule _)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"                {{");
            sb.AppendLine($"                    if (this.{propertyInfo.Name} is not null)");
            sb.AppendLine($"                    {{");
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
}
