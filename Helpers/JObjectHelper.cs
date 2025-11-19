using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Ihelpers.Helpers
{
    public static class JObjectHelper
    {
        /// <summary>
        /// Gets the value of a specified property from a JObject.
        /// </summary>
        /// <param name="wichObject">The JObject to retrieve the value from.</param>
        /// <param name="porpertyName">The name of the property to retrieve the value for.</param>
        /// <returns>The value of the specified property, or null if the property does not exist or the JObject is null.</returns>
        public static JToken GetJObjectValue(JObject? wichObject, string porpertyName)
        {
            JToken value = null;

            // Check if the JObject is not null
            if (wichObject != null)
            {
                // Try to get the value of the specified property
                wichObject.TryGetValue(porpertyName, out value);
            }

            return value;
        }

        /// <summary>
        /// Gets the value of a specified property from a JObject, converted to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <param name="wichObject">The JObject to retrieve the value from.</param>
        /// <param name="porpertyName">The name of the property to retrieve the value for.</param>
        /// <returns>The value of the specified property, converted to the specified type, or default(T) if the property does not exist or the JObject is null.</returns>
        public static T? GetJObjectValue<T>(JObject? wichObject, string porpertyName)
        {
            JToken value = null;

            // Check if the JObject is not null
            if (wichObject != null)
            {
                // Try to get the value of the specified property
                wichObject.TryGetValue(porpertyName, out value);
            }

            // Check if the value is not null
            if (value != null)
            {
                // Convert the value to the specified type
                return value.ToObject<T>();
            }

            // Return default value for the specified type if the value is null
            return default(T);
        }

        /// <summary>
        /// Gets the default metadata for a page.
        /// </summary>
        /// <returns>A JObject containing the default metadata for a page.</returns>
        public static JObject GetDefaultMeta()
        {
            // The default metadata as a JSON string
            string Meta = @"{
                    page:{
                     currentPage: 1,
                     hasNextPage: false,
                     hasPreviousPage: false,
                     perPage: 10,
                     total: 1,
                    }
                    }";

            // Return the default metadata as a JObject
            return JObject.Parse(Meta);
        }

        /// <summary>
        /// Tries to safely parse a given input to a JObject
        /// </summary>
        /// <param name="input">string that may contain a json object</param>
        /// <returns></returns>
        public static JObject? ParseOrNull(string? input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? null
                : JObject.Parse(input);
        }


        /// <summary>
        /// Tries to safely parse a given input to a Jtoken
        /// </summary>
        /// <param name="jInput">string that may contain a json object</param>
        /// <returns></returns>
        public static JToken? JTokenParseOrNull(string? jInput)
        {
            return string.IsNullOrWhiteSpace(jInput)
                ? null
                : JToken.Parse(jInput);
        }

        /// <summary>
        /// Ingore loop from Json
        /// </summary>
        /// <param name="_entity">string that may contain a json object</param>
        /// <returns></returns>
        public static string SerializeObjectSafe(dynamic? _entity)
        {
          var serializedJson =  JsonConvert.SerializeObject(_entity, Formatting.None,
                        new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        });

          return serializedJson;

        }


		public static T? IGet<T>(this JObject whichObj, string name)
		{
            if (whichObj.ContainsKey(name))
            {
				return whichObj.Value<T>(name);
			}
			
			return default(T);
		}

	}

    public static class JArrayHelper
    {
        /// <summary>
        /// Tries to safely parse a given input to a Jarray
        /// </summary>
        /// <param name="input">string that may contain a json object</param>
        /// <returns></returns>
        public static JArray? ParseOrNull(string? input)
        {
            return string.IsNullOrWhiteSpace(input)
                ? null
                : JArray.Parse(input);
        }

    }
}
