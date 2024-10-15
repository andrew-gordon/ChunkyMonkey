using ChunkyMonkey.Attributes;

namespace ChunkyMonkey.UnitTests.TestClasses
{
    [Chunk]
    public partial class ClassWithListProperty
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public string Name { get; set; }
        public int Age { get; set; }
        public List<int> Numbers { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }

}
