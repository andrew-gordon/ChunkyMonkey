namespace ChunkyMonkey.CodeGenerator
{
    internal class PropertyInfo
    {
        public string? Name { get; set; }
        public string? TypeName { get; set; }
        public bool IsArray { get; set; }
        public string? ArrayElementType { get; set; }
    }
}
