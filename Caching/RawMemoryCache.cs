using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ihelpers.Interfaces;
using Jitbit.Utils;
using Newtonsoft.Json;

namespace Ihelpers.Caching
{
    public class RawMemoryCache : ICacheBase
    {
        private readonly HashSet<string> _manuallyTrackedKeys;
        private readonly TimeSpan _defaultSlidingExpiration = TimeSpan.FromHours(1);

        // Fast cache instance
        private readonly FastCache<string, object> _fastCacheInstance;

        private JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public RawMemoryCache()
        {
            _manuallyTrackedKeys = new HashSet<string>();
            _fastCacheInstance = new FastCache<string, object>();

            if (Ihelpers.Extensions.ConfigContainer.cache is null)
            {
                Ihelpers.Extensions.ConfigContainer.cache = this;
            }
        }

        #region Main Cache Methods (Get, Create, Remove)

        // IMPORTANT NOTE: RawMemoryCache store pure objects.
        // GetValue will retrieve these objects directly.
        // CreateValue already stores pure objects.

        public Task<T> GetValue<T>(object key)
        {
            if (key == null) return Task.FromResult(default(T));
            string keyString = key.ToString();

            if (_fastCacheInstance.TryGet(keyString, out object valueFromCache))
            {
                // If the value is directly of type T (common for CreateValue)
                if (valueFromCache is T typedValue)
                {
                    return Task.FromResult(typedValue);
                }

                // If the value is a HashSet<object> (possibly from Remember)
                // and a single object of type T is expected.
                if (valueFromCache is HashSet<object> setObject)
                {
                    if (setObject.Any())
                    {
                        object firstItem = setObject.First();
                        if (firstItem is T typedFirstItem)
                        {
                            return Task.FromResult(typedFirstItem);
                        }
                    }
                }
                // The cached type doesn't match or couldn't be converted.
                return Task.FromResult(default(T));
            }
            return Task.FromResult(default(T)); // Not found
        }

        public Task<object?> GetValue(object key)
        {
            return GetValue<object>(key);
        }

        public Task<T> CreateValue<T>(object key, T value, double? expirationMinutes = null)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            string keyString = key.ToString();

            // RawMemoryCache stores the 'value' object directly.
            object valueToStore = value;

            if (expirationMinutes.HasValue)
            {
                DateTime absoluteExpiration = DateTime.UtcNow.AddMinutes(expirationMinutes.Value);
                _fastCacheInstance.AddOrUpdate(keyString, valueToStore, absoluteExpiration.TimeOfDay);
            }
            else
            {
                _fastCacheInstance.AddOrUpdate(keyString, valueToStore, _defaultSlidingExpiration);
            }
            _manuallyTrackedKeys.Add(keyString);
            return Task.FromResult(value);
        }

        public Task<object?> CreateValue(object key, object value, double? expirationTime = null)
        {
            return CreateValue<object>(key, value, expirationTime);
        }

        public Task<T> GetOrCreateValue<T>(object key, T valueToCreate)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            string keyString = key.ToString();

            if (_fastCacheInstance.TryGet(keyString, out object existingValue))
            {
                // Try to convert the existing value to type T.
                if (existingValue is T typedValue)
                {
                    return Task.FromResult(typedValue);
                }
                // If it's a HashSet<object> (from Remember) and T is not HashSet<object>,
                // try to take the first element.
                if (existingValue is HashSet<object> setObject)
                {
                    if (setObject.Any() && setObject.First() is T firstTypedItem)
                    {
                        return Task.FromResult(firstTypedItem);
                    }
                }
            }
            // Not found or incorrect type, so we create it.
            return CreateValue<T>(key, valueToCreate, null);
        }

        public Task Remove(object key)
        {
            if (key == null) return Task.CompletedTask;
            string keyString = key.ToString();
            _fastCacheInstance.Remove(keyString);
            _manuallyTrackedKeys.Remove(keyString);
            return Task.CompletedTask;
        }

        public Task Remove<T>(object key)
        {
            return Remove(key);
        }

        #endregion

        #region Manual Key Management (AddKey, GetKeys, RemoveKey)
        public void AddKey(string key)
        {
            _manuallyTrackedKeys.Add(key);
        }

        public IEnumerable<string> GetKeys()
        {
            return _manuallyTrackedKeys.ToList();
        }

        public bool RemoveKey(string key)
        {
            return _manuallyTrackedKeys.Remove(key);
        }
        #endregion

        public Task RemoveStartingWith(object keyPrefix)
        {
            if (keyPrefix == null) return Task.CompletedTask;
            string prefixString = keyPrefix.ToString();
            var keysToRemove = _manuallyTrackedKeys.Where(k => k.StartsWith(prefixString)).ToList();
            foreach (string keyToRemoveString in keysToRemove)
            {
                _fastCacheInstance.Remove(keyToRemoveString);
                _manuallyTrackedKeys.Remove(keyToRemoveString);
            }
            Console.WriteLine($"[FastCacheWrapper Warning] RemoveStartingWith for '{prefixString}' operates on manually tracked keys.");
            return Task.CompletedTask;
        }

        public Task AddTagsToUser(string userId, IEnumerable<string> tags)
        {
            Console.WriteLine("[FastCacheWrapper Warning] Tag functionality is complex and efficient implementation is limited with FastCache. This is a basic approximation.");
            var userTagsKey = $"user:{userId}:tags";
            HashSet<string> existingTags = null;
            if (_fastCacheInstance.TryGet(userTagsKey, out var val) && val is HashSet<string> hsVal)
            {
                existingTags = hsVal;
            }
            existingTags = existingTags ?? new HashSet<string>();

            foreach (var tag in tags) { existingTags.Add(tag); }
            // CreateValue will store 'existingTags' (HashSet<string>) directly.
            return CreateValue(userTagsKey, existingTags, null);
        }

        public Task RemoveTagsFromUser(string userId, IEnumerable<string> tags)
        {
            Console.WriteLine("[FastCacheWrapper Warning] Tag functionality is complex with FastCache.");
            var userTagsKey = $"user:{userId}:tags";
            HashSet<string> existingTags = null;
            if (_fastCacheInstance.TryGet(userTagsKey, out var val) && val is HashSet<string> hsVal)
            {
                existingTags = hsVal;
            }

            if (existingTags != null)
            {
                bool modified = false;
                foreach (var tag in tags) { if (existingTags.Remove(tag)) modified = true; }
                if (modified)
                {
                    if (existingTags.Any()) return CreateValue(userTagsKey, existingTags, null);
                    else return Remove(userTagsKey);
                }
            }
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<string>> GetKeysByTag(string entityFullName)
        {
            Console.WriteLine("[FastCacheWrapper Warning] GetKeysByTag is highly inefficient and limited with FastCache.");
            var matchingTagsFound = new List<string>();
            var currentTrackedUserTagKeys = _manuallyTrackedKeys.Where(k => k.StartsWith("user:") && k.EndsWith(":tags")).ToList();

            foreach (var userTagsKeyString in currentTrackedUserTagKeys)
            {
                // GetValue will retrieve the HashSet<string> directly.
                var tagsInCache = await GetValue<HashSet<string>>(userTagsKeyString);
                if (tagsInCache != null)
                {
                    foreach (var tag in tagsInCache)
                    {
                        if (tag.Contains(entityFullName))
                        {
                            matchingTagsFound.Add(tag);
                        }
                    }
                }
            }
            return matchingTagsFound.Distinct();
        }

        public async Task RemoveKeysContainingEntityFullName(string entityFullName)
        {
            Console.WriteLine("[FastCacheWrapper Warning] RemoveKeysContainingEntityFullName is limited.");
            IEnumerable<string> tagsAsKeysToRemove = await GetKeysByTag(entityFullName);
            foreach (var tagKey in tagsAsKeysToRemove)
            {
                await Remove(tagKey);
            }
        }

        public Task RemoveKeysContainingKey(string entityFullName)
        {
            Console.WriteLine("[FastCacheWrapper Error] RemoveKeysContainingKey is not reliably/efficiently implemented with FastCache.");
            return Task.FromException(new NotImplementedException("RemoveKeysContainingKey is not robustly implementable with FastCache without a complete redesign of the tag system."));
        }

        #region Inherited Methods 
        public void StoreKey(string key) { AddKey(key); }
        public IEnumerable<string> GetStoredKeys() { return GetKeys(); }
        public bool RemoveStoreKey(string key) { return RemoveKey(key); }
        #endregion

        #region Methods Implemented for Tags (Remember, Clear)
        /// <summary>
        /// Stores a value (raw object) with a specified key in the cache, and associates the specified tags with the cache entry.
        /// The data object is stored within a HashSet&lt;object&gt;, and the key itself points to this HashSet.
        /// Tags are stored as HashSets of keys (strings).
        /// </summary>
        /// <param name="key">The key of the cache entry.</param>
        /// <param name="data">The value (raw object) to store in the cache.</param>
        /// <param name="tags">A list of tags to associate with the cache entry.</param>
        public Task Remember(string key, object data, List<string> tags)
        {
            // 1. Store 'data' (raw object) in a "set" (HashSet<object>) associated with 'key'
            if (!_fastCacheInstance.TryGet(key, out var existingSetObject) || !(existingSetObject is HashSet<object> dataSet))
            {
                dataSet = new HashSet<object>();
            }
            dataSet.Add(data); // Stores the pure object
            _fastCacheInstance.AddOrUpdate(key, dataSet, _defaultSlidingExpiration);
            _manuallyTrackedKeys.Add(key); // Tracks the main data key

            // 2. For each tag, add 'key' to a "set" (HashSet<string>) associated with the tag
            foreach (string tag in tags)
            {
                if (!_fastCacheInstance.TryGet(tag, out var existingTagSetObject) || !(existingTagSetObject is HashSet<string> keySetForTag))
                {
                    keySetForTag = new HashSet<string>();
                }
                keySetForTag.Add(key); // Adds the main data key to the key set for this tag
                _fastCacheInstance.AddOrUpdate(tag, keySetForTag, _defaultSlidingExpiration);
                _manuallyTrackedKeys.Add(tag); // Tracks the tag key
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes all cache entries associated with the specified tags.
        /// It removes the data linked by the tags, the tags themselves,
        /// and any keys starting with the tag names.
        /// </summary>
        /// <param name="tags">A list of tags for which to remove cache entries.</param>
        public async Task Clear(List<string> tags)
        {
            foreach (string tag in tags)
            {
                // 1. Get all data keys associated with this tag
                if (_fastCacheInstance.TryGet(tag, out var tagSetObject) && tagSetObject is HashSet<string> dataKeysForTag)
                {
                    var keysToRemoveFromCache = new List<string>(dataKeysForTag); // Copy for safe iteration

                    foreach (string dataKey in keysToRemoveFromCache)
                    {
                        // 2. Remove the data (HashSet<object>) associated with each dataKey
                        _fastCacheInstance.Remove(dataKey);
                        _manuallyTrackedKeys.Remove(dataKey);
                    }
                }

                // 3. Remove the tag set itself (the mapping from tag to data keys)
                _fastCacheInstance.Remove(tag);
                _manuallyTrackedKeys.Remove(tag);

                // 4. Remove other keys that might start with the tag string
                await RemoveStartingWith(tag);
            }
        }
        #endregion
    }
}