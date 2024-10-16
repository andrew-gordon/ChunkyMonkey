namespace ChunkyMonkey.CodeGenerator.CodeGenerator
{
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
}
