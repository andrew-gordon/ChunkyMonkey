namespace ChunkyMonkey.UnitTests.Helpers
{
    internal class DictionaryComparer
    {
        public static bool Compare<TKey, TValue>(Dictionary<TKey, TValue> dict1, Dictionary<TKey, TValue> dict2) where TKey : notnull
        {
            // Check if both dictionaries are null or the same reference
            if (ReferenceEquals(dict1, dict2)) return true;

            // Check if either is null
            if (dict1 == null || dict2 == null) return false;

            // Check if the dictionaries have the same number of elements
            if (dict1.Count != dict2.Count) return false;

            // Compare key-value pairs
            return dict1.All(kvp => dict2.TryGetValue(kvp.Key, out var value) && EqualityComparer<TValue>.Default.Equals(kvp.Value, value));
        }
    }
}
