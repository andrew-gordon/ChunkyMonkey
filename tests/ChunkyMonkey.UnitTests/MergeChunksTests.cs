using ChunkyMonkey.UnitTests.Helpers;
using ChunkyMonkey.UnitTests.TestClasses;

namespace ChunkyMonkey.UnitTests
{
    public partial class MergeChunksTests
    {
        [Fact]
        public void MergeChunks_ArrayProperties_ReturnsChunkedInstances()
        {
            // Arrange
            var chunks = new List<ClassWithArrayProperty>
            {
                new() {
                    Name = "John",
                    Age = 25,
                    Numbers = [1, 2, 3]
                },
                new() {
                    Name = "John",
                    Age = 25,
                    Numbers = [4, 5, 6]
                },
                new() {
                    Name = "John",
                    Age = 25,
                    Numbers = [7, 8, 9]
                },
                new() {
                    Name = "John",
                    Age = 25,
                    Numbers = [10]
                },

                // This chunk wouldn't be emitted by the generator, but checking it for completeness.
                new() {
                    Name = "John",
                    Age = 25,
                    Numbers = []
                }
            };

            // Act
            var actual = ClassWithArrayProperty.MergeChunks(chunks);

            var expected = new ClassWithArrayProperty
            {
                Name = "John",
                Age = 25,
                Numbers = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
            };

            // Assert
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Age, actual.Age);
            Assert.True(expected.Numbers.SequenceEqual(actual.Numbers));
        }

        [Fact]
        public void MergeChunks_MulitpleCollectionProperties_ReturnsChunkedInstances()
        {
            // Arrange
            var chunks = new List<ClassWithMultipleCollectionProperties>
            {
                new() {
                    Name = "John",
                    Age = 25,
                    FavouriteNumbers = [1, 2, 3],
                    FavouriteFilms = [ "Reservoir Dogs", "Pulp Fiction" ],
                    LotteryNumbers = [1,2,3,4,5,6],
                    Attributes = new Dictionary<string, string>
                    {
                        { "Occupation", "Developer" },
                        { "Location", "USA" },
                        { "Hobbies", "Programming" }
                    }
                },
                new() {
                    Name = "John",
                    Age = 25,
                    FavouriteNumbers = [4, 5, 6],
                    FavouriteFilms = [ "Inception", "The Matrix" ],
                    LotteryNumbers = [11,12,13,14,15,16],
                    Attributes = new Dictionary<string, string>
                    {
                        { "Favourite Colour", "Red" },
                    }
                },
                new() {
                    Name = "John",
                    Age = 25,
                    FavouriteNumbers = [7, 8, 9],
                    FavouriteFilms = [ "The Shawshank Redemption", "The Godfather" ],
                    LotteryNumbers = [21,22,23,24,25,26],
                    Attributes = new Dictionary<string, string>
                    {
                        { "Favourite Biscuit", "Custard Cream" }
                    }
                },
                new() {
                    Name = "John",
                    Age = 25,
                    FavouriteNumbers = [10],
                    FavouriteFilms = [ "The Dark Knight", "Fight Club"],
                    LotteryNumbers = [31,32,33,34,35,36],
                    Attributes = []
                },

                // This chunk wouldn't be emitted by the generator, but checking it for completeness.
                new() {
                    Name = "John",
                    Age = 25,
                    FavouriteNumbers = [],
                    FavouriteFilms = [],
                    LotteryNumbers = [],
                    Attributes = []
                }
            };

            // Act
            var actual = ClassWithMultipleCollectionProperties.MergeChunks(chunks);

            var expected = new ClassWithMultipleCollectionProperties
            {
                Name = "John",
                Age = 25,
                FavouriteNumbers = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
                FavouriteFilms = ["Reservoir Dogs", "Pulp Fiction", "Inception", "The Matrix", "The Shawshank Redemption", "The Godfather", "The Dark Knight", "Fight Club"],
                LotteryNumbers = [1, 2, 3, 4, 5, 6, 11, 12, 13, 14, 15, 16, 21, 22, 23, 24, 25, 26, 31, 32, 33, 34, 35, 36],
                Attributes = new Dictionary<string, string>
                {
                    { "Occupation", "Developer" },
                    { "Location", "USA" },
                    { "Hobbies", "Programming" },
                    { "Favourite Colour", "Red" },
                    { "Favourite Biscuit", "Custard Cream" }
                }
            };

            // Assert
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Age, actual.Age);
            Assert.True(expected.FavouriteNumbers.SequenceEqual(actual.FavouriteNumbers));
            Assert.True(expected.FavouriteFilms.SequenceEqual(actual.FavouriteFilms));
            Assert.True(expected.LotteryNumbers.SequenceEqual(actual.LotteryNumbers));
            Assert.True(DictionaryComparer.Compare(expected.Attributes, actual.Attributes));
        }
    }
}
