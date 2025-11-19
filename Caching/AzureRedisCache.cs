using Ihelpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ihelpers.Caching
{
    using System;
    using StackExchange.Redis;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using TypeSupport.Assembly;
    using Microsoft.EntityFrameworkCore.Metadata.Internal;
    using Ihelpers.Helpers;
    using Microsoft.AspNetCore.Http;
    using System.Dynamic;
    using System.Data;
    using Microsoft.EntityFrameworkCore.Metadata;
    using System.Security.Cryptography;

    /// <summary>
    /// AzureRedisCache provides caching functionality using Azure Redis as the caching backend.
    /// </summary>
    public class AzureRedisCache : ICacheBase
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;


        private JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        /// <summary>
        /// Initializes a new instance of the AzureRedisCache class.
        /// </summary>
        public AzureRedisCache()
        {
            string? connString = ConfigurationHelper.GetConfig<string?>("ConnectionStrings:AzureRedisCache");


//#if DEBUG
//            var configurationOptions = ConfigurationOptions.Parse(connString);

//            configurationOptions.ConnectTimeout = 50000;
//            configurationOptions.SyncTimeout = 100000;
//            configurationOptions.AsyncTimeout = 100000;

//            _redis = ConnectionMultiplexer.Connect(configurationOptions);
//            _database = _redis.GetDatabase();

//#else
  _redis = ConnectionMultiplexer.Connect(connString);
            _database = _redis.GetDatabase();
//#endif
        }

        /// <summary>
        /// Creates or updates a value in the cache with the specified key, value, and optional expiration time.
        /// </summary>
        /// <typeparam name="T">The type of the value to store in the cache.</typeparam>
        /// <param name="key">The key of the cache entry.</param>
        /// <param name="value">The value to store in the cache.</param>
        /// <param name="expirationTime">Optional expiration time in seconds for the cache entry.</param>
        /// <returns>The stored value.</returns>
        public async Task<T> CreateValue<T>(object key, T value, double? expirationTime = null)
        {
            TimeSpan? expiry = expirationTime.HasValue ? TimeSpan.FromSeconds(expirationTime.Value) : TimeSpan.FromSeconds(ConfigurationHelper.GetConfig<double?>("DefaultConfigs:Caching:TimeAmmount") ?? 60d);

            _database.StringSet(key.ToString(), Serialize(value), expiry);

            return value;
        }

        /// <summary>
        /// Creates or updates a value in the cache with the specified key, value, and optional expiration time.
        /// </summary>
        /// <param name="key">The key of the cache entry.</param>
        /// <param name="value">The value to store in the cache.</param>
        /// <param name="expirationTime">Optional expiration time in seconds for the cache entry.</param>
        /// <returns>The stored value, or null if the value is not serializable.</returns>
        public async Task<object?> CreateValue(object key, object value, double? expirationTime = null)
        {
            TimeSpan? expiry = expirationTime.HasValue ? TimeSpan.FromSeconds(expirationTime.Value) : TimeSpan.FromSeconds(ConfigurationHelper.GetConfig<double?>("DefaultConfigs:Caching:TimeAmmount") ?? 60d);
            _database.StringSet(key.ToString(), Serialize(value), expiry);
            return value;
        }

        /// <summary>
        /// Retrieves a value from the cache with the specified key.
        /// </summary>
        /// <param name="key">The key of the cache entry.</param>
        /// <returns>The value from the cache, or null if the key is not found.</returns>
        public async Task<object?> GetValue(object key)
        {
            try
            {
                var redisValue = await _database.StringGetAsync(key.ToString());
                return redisValue.IsNullOrEmpty ? null : Deserialize<object>(redisValue);
            }
            catch
            {
                try
                {
                    var server = GetServer();
                    var keys = server.Keys(pattern: key.ToString() + "*");
                    var result = await _database.SetMembersAsync(key.ToString());
                    var objResult = Deserialize<object>(result.First());
                    return objResult;
                }
                catch
                {
                    return null;
                }

            }
        }

        /// <summary>
        /// Retrieves a value of
        /// a specified type from the cache with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value to retrieve from the cache.</typeparam>
        /// <param name="key">The key of the cache entry.</param>
        /// <returns>The value from the cache, or the default value of T if the key is not found.</returns>
        public async Task<T?> GetValue<T>(object key)
        {

            try
            {
                RedisValue redisValue = _database.StringGet(key.ToString());
                return redisValue.IsNullOrEmpty ? default(T) : Deserialize<T?>(redisValue);
            }
            catch
            {
                var server = GetServer();
                var keys = server.Keys(pattern: key.ToString() + "*");
                var result = await _database.SetMembersAsync(key.ToString());
                var objResult = result.Any() ? Deserialize<T>(result.First()) : default(T);
                return objResult;
            }

        }

        /// <summary>
        /// Removes a cache entry with the specified key.
        /// </summary>
        /// <typeparam name="T">The type of the value to remove from the cache.</typeparam>
        /// <param name="key">The key of the cache entry to remove.</param>
        public async Task Remove<T>(object key)
        {
            _database.KeyDelete(key.ToString());
        }

        /// <summary>
        /// Removes all cache entries that start with the specified key.
        /// </summary>
        /// <param name="key">The key prefix of the cache entries to remove.</param>
        public async Task RemoveStartingWith(object key)
        {
            var server = GetServer();
            var keys = server.Keys(pattern: key.ToString() + "*");
            foreach (var k in keys)
            {
                _database.KeyDelete(k);
            }
        }

        /// <summary>
        /// Gets the server for the Azure Redis Cache.
        /// </summary>
        /// <returns>An instance of the server behind IServer interface.</returns>
        private IServer GetServer()
        {
            var endpoint = _redis.GetEndPoints().First();
            return _redis.GetServer(endpoint);
        }

        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        private string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Deserializes a JSON string to an object of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="serialized">The JSON string to deserialize.</param>
        /// <returns>An object of the specified type.</returns>
        private T Deserialize<T>(string serialized)
        {
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        /// <summary>
        /// Stores a value with a specified key in the cache, and associates the specified tags with the cache entry.
        /// </summary>
        /// <param name="key">The key of the cache entry.</param>
        /// <param name="data">The value to store in the cache.</param>
        /// <param name="tags">A list of tags to associate with the cache entry.</param>
        public async Task Remember(string key, object data, List<string> tags)
        {


            // Add to main table
            await _database.SetAddAsync(key, JsonConvert.SerializeObject(data, _jsonSerializerSettings));

            // Create the rest of the tags
            foreach (string tag in tags)
            {
                await _database.SetAddAsync(tag, key);
            }


        }

        /// <summary>
        /// Removes all cache entries associated with the specified tags.
        /// </summary>
        /// <param name="tags">A list of tags for which to remove cache entries.</param>
        public async Task Clear(List<string> tags)
        {
            foreach (string tag in tags)
            {

                var tagKeys = await _database.SetMembersAsync(tag.ToString());

                foreach (var tagKey in tagKeys)
                {
                    await _database.KeyDeleteAsync(tagKey.ToString());
                }

                await _database.KeyDeleteAsync(tag.ToString());

                await RemoveStartingWith(tag);

            }

            //also delete keys



        }


    }

}