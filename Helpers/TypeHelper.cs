using Ihelpers.DataAnotations;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using TypeSupport.Extensions;

namespace Ihelpers.Helpers
{
    public static class TypeHelper
    {
        /// <summary>
        /// Gets the properties with [Relational_field] Data annotation of a given class
        /// </summary>
        /// <param name="wichtype"></param>
        /// <returns></returns> 
        public static async Task<List<string>> getRelationsOfClass(Type wichtype)
        {
            List<string> relations = new List<string>();

            var propList = wichtype.GetProperties().ToList();

            foreach (PropertyInfo prop in propList)
            {
                var relational = prop.GetCustomAttributes(true).Where(att => att is IDataAnnotationBase && att is RelationalField).FirstOrDefault();

                if (relational != null)
                {
                    relations.Add(prop.Name);
                }

            }
            return relations;
        }
        /// <summary>
        /// return the base type of a listOf<T>
        /// </summary>
        /// <param name="wichIQueryable"></param>
        /// <returns></returns>
        public static async Task<Type> getTypeOfQuery(dynamic wichIQueryable)
        {
            return wichIQueryable.GetType().GenericTypeArguments[0];
        }
        /// <summary>
        /// checks if a value is numeric or not, best performance aplied
        /// </summary>
        /// <param name="wichFilter"></param>
        /// <returns></returns>
        public static async Task<bool> isNumericValue(dynamic wichValue)
        {
            string wichValueString = wichValue.ToString();

            if (wichValueString.All(c => (c >= 48 && c <= 57)))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        /// <summary>
        /// Get the fields of a given entityType that are numeric or not
        /// </summary>
        /// <param name="wichtype"></param>
        /// <param name="Numerics"></param>
        /// <returns></returns>
        public static async Task<List<string>> getFieldsMatchingTypeFilter(Type wichEntity, bool Numerics)
        {
            List<string> entityFieldsMatchingTypeFilter = new List<string>();

            var propList = wichEntity.GetProperties().ToList();

            foreach (PropertyInfo prop in propList)
            {

                var typeofProperty = prop.PropertyType;

                var extendedType = typeofProperty.GetExtendedType();

                if (extendedType.IsNumericType && Numerics == true)
                {
                    entityFieldsMatchingTypeFilter.Add(prop.Name);
                }
                else if (extendedType.IsNumericType == false && Numerics == false)
                {
                    entityFieldsMatchingTypeFilter.Add(prop.Name);
                }

            }
            return entityFieldsMatchingTypeFilter;
        }

        /// <summary>
        /// Gets the matching type of given string filter
        /// </summary>
        /// <param name="wichFinter"></param>
        /// <returns></returns>
        public static async Task<Type> getTypeOfFilter(dynamic wichFinter)
        {
            return typeof(string);
        }
        /// <summary>
        /// Returns the class name of given object 
        /// </summary>
        /// <returns></returns>

        public static async Task<string> getClassName(dynamic wichObject)
        {
            return wichObject.GetType().Name;
        }

        /// <summary>
        /// Dictionary to CSV
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IEnumerable<string> ToCsv(this Dictionary<string, List<object>> data, char separator = '\t', JArray? jColumns = null)
        {
            List<string>? columns = jColumns?.ToObject<List<string>>()?? data.Keys.ToList(); 

            // Quotation if required: 123,45 -> "123,45"; a"bc -> "a""bc" 
            string Quote(object value) => string.IsNullOrEmpty($"{value}")
                ? "" : $"{value}".Contains(',') || $"{value}".Contains('"')
                ? "\"" + $"{value}".Replace("\"", "\"\'") + "\""
                  : $"{value}";

            // Captions
            yield return string.Join(separator, columns.Select(col => Quote(col)));

            // Rows one after one 
            int rowCount = data.Max(pair => pair.Value.Count);
            for (int r = 0; r < rowCount; ++r)
            {
                yield return string.Join(separator, columns.Select(col =>
                {
                    if (!data.ContainsKey(col)) return Quote("");
                    if (r >= data[col].Count) return Quote("");
                    return Quote(data[col][r]);
                }));
            }


        }

        
        /// <summary>
        /// Dictionary to JSON Report
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static List<JObject> ToJsonReport(this Dictionary<string, List<object?>> data, JArray? orderedFields = null)
        {

            List<string>? orderedKeys = orderedFields?.ToObject<List<string>>() ?? null;

            List<string> Keys = orderedKeys ?? data.Keys.ToList();

            Keys.AddRange(data.Where(header => !Keys.Contains(header.Key)).Select(header => header.Key).ToList());

            //Create the Json Object
            List<JObject> response = new List<JObject>();

            int recordsFound = data.First().Value.Count();

            for(int rowIndex = 0; rowIndex < recordsFound; rowIndex++)
            {
               
                JObject record = new JObject();
                foreach (var key in Keys)
                {
                    if (data.ContainsKey(key)) 
                    {
                        record[key] = data[key][rowIndex] != null ? JToken.FromObject(data[key][rowIndex]) : null;

                    }

                }
                response.Add(record);
            }
            return response;
        }

        /// <summary>
        /// Returns the base tyoe of dymanic objects
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Type GetUnknowType(this object obj)
		{
			Type? type = null;
			if (isList(obj))
			{
				type = obj.GetType().GetGenericArguments().Single();
			}
			else
			{
				type = obj.GetType();
			}
            return type;
		}


        /// <summary>
        /// Determines if a dynamic object is a list
        /// </summary>
        /// <param name="input">Set of object to identify</param>
        /// <returns>true if input is list, false if input is not a list</returns>
       public static bool isList(dynamic input)
        {
            //only a list supports this functions (including empty lists)
            object obj = input;
            Type type = obj?.GetType();
            bool isList = type != null
                       && type.IsGenericType
                       && (type.GetGenericTypeDefinition() == typeof(List<>) || type.GetGenericTypeDefinition().Name.Contains("List"));
            //if is a list return true
            return isList;

        }


        /// <summary>
		/// Gets a list of property names for the specified object, excluding certain properties from EntityBase.
		/// </summary>
		/// <param name="obj">The object to retrieve properties from.</param>
		/// <returns>A list of property names for the specified object, or null if the object is null.</returns>
		public static List<string>? GetProperties(this object obj)
        {
            // Declare variables for the type, properties, and response.
            Type? type = null;
            List<PropertyInfo> properties = null;
            List<string>? response = null;

            // Check if the object is not null.
            if (obj != null)
            {
                // Get the unknown type of the object.
                type = obj.GetUnknowType();

                // Get all the properties of the type and exclude certain properties.
                var validProps = type.GetProperties().Where(propp => !propp.Name.Contains("_id") && !propp.Name.Contains("_at") && !propp.Name.Contains("_by")).ToList();
                validProps = validProps.Where(at => at.GetCustomAttributes().ToList().Where(att => att is RelationalField || att is ObjectToString || att is SimpleObjectToString).FirstOrDefault() == null).ToList();

                // Check if any valid properties were found.
                if (validProps.Any())
                {
                    // Find the index of the "id" property.
                    int indexOfId = validProps.IndexOf(validProps.Where(prop => prop.Name == "id").First()) + 1;

                    // Get the names of the properties until index of ide, to avoid entityBase properties
                    response = validProps.Take(indexOfId).Select(prop => prop.Name).ToList();
                }
            }

            // Return the list of property names.
            return response;
        }
        /// <summary>
        /// Gets properties path for given object in camelCase format
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Dictionary<string, string>? GetPropertiesPath(this object obj)
        {
            Dictionary<string, string>? response = null;

            var props = obj.GetProperties();

            if (props.Any()) {
                response = new Dictionary<string, string>();
                props.ForEach(prop => { response.Add(prop.ToCamelCase(), string.Empty); });

            }

            return response;
        }
        /// <summary>
        /// Returns the type for generic list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_"></param>
        /// <returns></returns>
        public static Type GetListType<T>(this List<T> _)
        {
            return typeof(T);
        }

        /// <summary>
        /// Gets the ids of given list inside dynamic object
        /// </summary>
        /// <param name="wichValue"></param>
        /// <returns></returns>
		public static List<long> GetIds(dynamic wichValue)
		{
            List<long> ids = new List<long>();
            foreach(dynamic wich in wichValue)
            {
                ids.Add(wich.id);
            }
            return ids;
		}
       /// <summary>
       /// Obtain a type of class inside Ihelpers Module
       /// </summary>
       /// <param name="iHelper"></param>
       /// <returns></returns>
        public static Type? GetTypeOf(string iHelper)
        {
            return Type.GetType(iHelper);
        } 
	}
}
