namespace Ihelpers.Helpers
{
    /// <summary>
    /// A static class containing helper methods for dictionaries.
    /// </summary>
    public static class DictionaryHelper
    {
        /// <summary>
        /// Attempts to get the value with the specified key from the specified dictionary, if value is not present return null.
        /// </summary>
        /// <typeparam name="T">The type of the keys and values in the dictionary.</typeparam>
        /// <param name="wichDict">The dictionary to search for the value in.</param>
        /// <param name="wichKey">The key of the value to retrieve.</param>
        /// <returns>The value with the specified key, if it exists in the dictionary; otherwise, `null`.</returns>
        public static object? TryGetValue<T>(Dictionary<T, T> wichDict, string wichKey) where T : class
        {
            try
            {
                return wichDict[wichKey as T];
            }
            catch
            {
                return null;
            }
        }
    }
}
