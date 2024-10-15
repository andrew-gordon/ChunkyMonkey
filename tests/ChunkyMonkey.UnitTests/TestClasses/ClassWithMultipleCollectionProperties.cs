using ChunkyMonkey.Attributes;
using System.Collections.ObjectModel;

namespace ChunkyMonkey.UnitTests.TestClasses
{
    [Chunk]
    public partial class ClassWithMultipleCollectionProperties
    {
        public string? Name { get; set; }
        public int Age { get; set; }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Dictionary<string, string> Attributes { get; set; }
        public Collection<int> FavouriteNumbers { get; set; }
        public List<int> LotteryNumbers { get; set; }
        public string[] FavouriteFilms { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }

}
