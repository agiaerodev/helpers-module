using Ihelpers.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace Ihelpers.Caching
{
    /// <summary>
    /// This class is a base class for memory cache implementation.
    /// It implements `ICacheBase` interface and uses `IMemoryCache` to store data.
    /// </summary>
    public class MemoryCache : ICacheBase
    {


        private readonly HashSet<string> _cacheKeys;


        public void AddKey(string key)
        {
            _cacheKeys.Add(key);
        }

        public IEnumerable<string> GetKeys()
        {
            return _cacheKeys;
        }

        public bool RemoveKey(string key)
        {
            return _cacheKeys.Remove(key);
        }

        /// <summary>
        /// An instance of `IMemoryCache` to store data.
        /// </summary>
        public IMemoryCache _cache;

        /// <summary>
        /// Cache entry options for cache data.
        /// </summary>
        private MemoryCacheEntryOptions cacheOptions;

        /// <summary>
        /// Constructor to initialize the cache.
        /// </summary>
        public MemoryCache()
        {
            // Initialize cache options with a sliding expiration of 1 hour
            cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = TimeSpan.FromHours(1)
            };

            // Initialize an instance of `IMemoryCache`
            var cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new MemoryCacheOptions());
            _cache = cache;

            // Set the cache instance in the `ConfigContainer` if not already set
            if (Ihelpers.Extensions.ConfigContainer.cache is null) Ihelpers.Extensions.ConfigContainer.cache = this;

            _cacheKeys = new HashSet<string>();
        }

        /// <summary>
        /// Removes an item from cache based on its key.
        /// </summary>
        /// <typeparam name="T">Type of item stored in cache</typeparam>
        /// <param name="key">Key to identify the item in cache</param>
        public async Task Remove<T>(object key)
        {
            // Ensure that item exists to avoid exception throwing 
            object? internalValue = GetValueInternal<T>(key);

            // Remove the item if it exists in cache
            if (internalValue != null)
            {
                _cache.Remove(key);
                _cacheKeys.Remove(key.ToString());
            }
        }

        /// <summary>
        /// Removes an item from cache based on its key.
        /// </summary>
        /// <param name="key">Key to identify the item in cache</param>
        public async Task Remove(object key)
        {
            // Ensure that item exists to avoid exception throwing 
            object? internalValue = GetValueInternal<object>(key);

            // Remove the item if it exists in cache
            if (internalValue != null)
            {
                _cache.Remove(key);
                _cacheKeys.Remove(key.ToString());
            }
        }

        /// <summary>
        /// Tries to get a value from cache, or create it if not exists.
        /// </summary>
        /// <typeparam name="T">The type of the value to get or create.</typeparam>
        /// <param name="key">The key of the value to get or create.</param>
        /// <param name="value">The value to set in cache if not exists.</param>
        /// <returns>The value from cache.</returns>
        public async Task<T> GetOrCreateValue<T>(object key, T value)
        {
            T internalValue = await GetValueInternal<T>(key);

            if (internalValue == null)
            {
                _cache.Set<T>(key, value, cacheOptions);

                return value;
            }
            else
            {
                return internalValue;
            }
        }

        /// <summary>
        /// Creates a value in cache.
        /// </summary>
        /// <param name="key">The key of the value to create.</param>
        /// <param name="value">The value to create.</param>
        /// <param name="expirationTime">The expiration time for the cache entry. If specified, overrides the default `cacheOptions`.</param>
        /// <returns>The created value.</returns>
        public async Task<object?> CreateValue(object key, object value, double? expirationTime = null)
        {
            //verify if special entryOption was set
            if (expirationTime != null) cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(expirationTime.Value)
            };

            //verify if value exists first to avoid exceptions
            object? internalValue = GetValueInternal<object>(key);

            //if value exists then delete it
            if (internalValue != null) _cache.Remove(key);

            //create the value again
            _cache.Set(key, value, cacheOptions);

            _cacheKeys.Add(key.ToString());

            return value;
        }

        public async Task<T> CreateValue<T>(object key, T value, double? expirationTime = null)
        {
            //verify if special entryOption was set
            if (expirationTime != null) cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(expirationTime.Value)
            };

            //verify if value exists first to avoid exceptions
            T? internalValue = await GetValueInternal<T>(key);

            //if value exists then delete it
            if (internalValue != null) _cache.Remove(key);

            //create the value again
            _cache.Set<T>(key, value, cacheOptions);

            _cacheKeys.Add(key.ToString());

            return value;
        }

        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key, or default(object) if the key is not found.</returns>
        public async Task<object?> GetValue(object key)
        {
            return await GetValue<object>(key);
        }


        /// <summary>
        /// Gets the value associated with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value to get.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key, or default(T) if the key is not found.</returns>
        public async Task<T> GetValue<T>(object key)
        {
            return await GetValueInternal<T>(key);
        }

        /// <summary>
        /// Gets the value associated with the specified key, meant to be used only inside this class.
        /// </summary>
        /// <typeparam name="T">The type of the value to get.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value associated with the specified key, or default(T) if the key is not found.</returns>
        private async Task<T> GetValueInternal<T>(object key)
        {
            if (key == null) return default(T);

            T internalValue;

            // Attempt to get the value from the cache
            _cache.TryGetValue<T>(key, out internalValue);

            return internalValue;
        }

        public async Task RemoveStartingWith(object key)
        {
            var keysToRemove = _cacheKeys.Where(ke => ke.StartsWith(key.ToString())).ToList();

            foreach (string keytoRemove in keysToRemove)
            {
                Remove(keytoRemove);
            }

        }



        public void StoreKey(string key)
        {
            _cacheKeys.Add(key);
        }

        public IEnumerable<string> GetStoredKeys()
        {
            return _cacheKeys;
        }

        public bool RemoveStoreKey(string key)
        {
            return _cacheKeys.Remove(key);
        }

        public async Task AddTagsToUser(string userId, IEnumerable<string> tags)
        {
            var userTagsKey = $"user:{userId}:tags";
            var existingTags = await GetValueInternal<HashSet<string>>(userTagsKey) ?? new HashSet<string>();
            foreach (var tag in tags)
            {
                existingTags.Add(tag);
            }
            await CreateValue(userTagsKey, existingTags);
        }

        public async Task RemoveTagsFromUser(string userId, IEnumerable<string> tags)
        {
            var userTagsKey = $"user:{userId}:tags";
            var existingTags = await GetValueInternal<HashSet<string>>(userTagsKey);
            if (existingTags != null)
            {
                foreach (var tag in tags)
                {
                    existingTags.Remove(tag);
                }
                await CreateValue(userTagsKey, existingTags);
            }
        }

        public async Task<IEnumerable<string>> GetKeysByTag(string entityFullName)
        {
            var matchingKeys = new List<string>();
            var userKeys = _cacheKeys.Where(key => key.StartsWith("user:") && key.EndsWith(":tags")).ToList();

            foreach (var userKey in userKeys)
            {
                var userTags = await GetValueInternal<HashSet<string>>(userKey);
                if (userTags != null)
                {
                    var keys = userTags.Where(tag => tag.Contains(entityFullName)).ToList();
                    matchingKeys.AddRange(keys);
                }
            }

            return matchingKeys;
        }

        public async Task RemoveKeysContainingEntityFullName(string entityFullName)
        {
            var matchingKeys = await GetKeysByTag(entityFullName);

            // Eliminar las claves que coinciden con los tags
            foreach (var key in matchingKeys)
            {
                await Remove(key);
            }
        }

        public async Task RemoveKeysContainingKey(string entityFullName)
        {
            var userKeys = _cacheKeys.Where(key => key.StartsWith("user:") && key.EndsWith(":tags")).ToList();

            foreach (var userKey in userKeys)
            {
                var userTags = await GetValueInternal<HashSet<string>>(userKey);
                if (userTags != null)
                {
                    var matchingTags = userTags.Where(tag => tag.Contains(entityFullName)).ToList();
                    if (matchingTags.Count > 0)
                    {
                        // Eliminar las keys que coinciden con los tags
                        foreach (var tag in matchingTags)
                        {
                            await Remove(tag.ToString());
                        }

                        // Eliminar los tags del usuario
                        var userId = userKey.Split(':')[1];
                        await RemoveTagsFromUser(userId, matchingTags);
                    }
                }
            }
        }

        public Task Remember(string key, object data, List<string> tags)
        {
            throw new NotImplementedException();
        }

        public Task Clear(List<string> tags)
        {
            throw new NotImplementedException();
        }
    }
}