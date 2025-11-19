namespace Ihelpers.Interfaces
{
    /// <summary>
    /// The `ICacheBase` interface provides basic cache functionalities.
    /// </summary>
    public interface ICacheBase
    {
        /// <summary>
        /// Removes a value from the cache, based on its key.
        /// </summary>
        /// <typeparam name="T">The type of the value to be removed.</typeparam>
        /// <param name="key">The key of the value to be removed.</param>
        public Task Remove<T>(object key);

        public Task RemoveStartingWith(object key);

        /// <summary>
        /// Creates a new value in the cache, with the given key and expiration time.
        /// </summary>
        /// <typeparam name="T">The type of the value to be created.</typeparam>
        /// <param name="key">The key of the value to be created.</param>
        /// <param name="value">The value to be created.</param>
        /// <param name="ExpirationTime">The optional expiration time for the value (in minutes).</param>
        /// <returns>The value that was created in the cache.</returns>
        public Task<T> CreateValue<T>(object key, T value, double? ExpirationTime = null);

        /// <summary>
        /// Creates a new value in the cache, with the given key and expiration time in minutes.
        /// </summary>
        /// <param name="key">The key of the value to be created.</param>
        /// <param name="value">The value to be created.</param>
        /// <param name="ExpirationTime">The optional expiration time for the value (in minutes).</param>
        /// <returns>The value that was created in the cache.</returns>
        public Task<object> CreateValue(object key, object value, double? ExpirationTime = null);

        /// <summary>
        /// Gets the value from the cache, based on its key.
        /// </summary>
        /// <param name="key">The key of the value to be retrieved.</param>
        /// <returns>The value that was retrieved from the cache, or `null` if the value was not found.</returns>
        public Task<object?> GetValue(object key);

        /// <summary>
        /// Gets the value from the cache, based on its key.
        /// </summary>
        /// <typeparam name="T">The type of the value to be retrieved.</typeparam>
        /// <param name="key">The key of the value to be retrieved.</param>
        /// <returns>The value that was retrieved from the cache, casted to the specified type.</returns>
        public Task<T> GetValue<T>(object key);

        //public Task ClearCache(List<string>? tags);
        //public Task ClearCache(string? key);


        public Task Remember(string key, object data, List<string> tags);



        public Task Clear(List<string> tags);



    }
}