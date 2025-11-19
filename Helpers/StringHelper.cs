using Microsoft.AspNetCore.Http;
using MoreLinq;
using Newtonsoft.Json.Serialization;
using PluralizeService.Core;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.XPath;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Ihelpers.Helpers
{
    public static class StringHelper
    {

        public static string Capitalize(string str){
            return str.First().ToString().ToUpper() + str.Substring(1);
        }

        public static string Lowercase(string str)
        {
            return str.First().ToString().ToLower() + str.Substring(1);
        }
        /// <summary>
        /// converts snake_case_string to camelCase, meant to be used in a JsonConvert contract resolver
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToCamelCase(this string? str) => str is null
        ? null
        : new DefaultContractResolver()
        {

            NamingStrategy = new CamelCaseNamingStrategy()


        }.GetResolvedPropertyName(SnakeParser(str));

        /// <summary>
        /// converts snake_case_string to camelCaseString, meant to be used inside tranjsformerbase class
        /// </summary>
        /// <param name="Snake_CaseStr">string that wil lbe converted to camel case</param>
        /// <returns></returns>
        public static string SnakeParser(string Snake_CaseStr)
        {
            //Char that will be reference for split the string
            char delimiter = '_';
            //verify if is necessary the split
            if (Snake_CaseStr.Contains(delimiter))
            {
                StringBuilder finalWord = new StringBuilder();

                //string to be returned with best performance

                //split the input_string by '_' char into a list
                List<string> words = Snake_CaseStr.Split(delimiter).ToList();

                //foreach word in the list
                foreach (var word in words)
                {
                    //first word doesnt need to be uppercase 
                    if (words.IndexOf(word) == 0)
                    {
                        //add the 1st word to the string builder
                        finalWord.Append(word);
                    }
                    else
                    {
                        //the other words must be uppercase
                        finalWord.Append(char.ToUpper(word[0]) + word.Substring(1));
                    }

                }

                //return the processed word
                return finalWord.ToString();


            }
            //return the unprocessed word
            return Snake_CaseStr;
        }

        public static string ToSnakeCase(this string? str) => str is null
        ? null
        : new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() }.GetResolvedPropertyName(str);


        /// <summary>
        /// check if string is a valid method array
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsValidArray(this string? str)
        {
            str = str.Trim();
            return str != null && str.StartsWith("[") && str.EndsWith("]") && str.Length >= 2;

        }

        /// <summary>
        /// check if string is a valid method array
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsValidJson(this string? str)
        {
            str = str.Trim();
            return str != null && str.StartsWith("{") && str.EndsWith("}") && str.Length >= 2;

        }

        /// <summary>
        /// Removes json symbols and constructs the unformatted path of the property
        /// </summary>
        /// <param name="str">Property string unformatted exmp: "workdayTransactions" : "" ,"</param>
        /// <returns>"workdayTransactions"</returns>
        public static string Unformatted(this string? str)
        
        {

            string response = string.Empty;

            var namespaces = str.Split(':').First().Split('.').ToArray();

            if (namespaces.Length == 0) return string.Empty;

            var finalNamespace = new List<string>();

            foreach (var line in namespaces)
            {
                if (line.Contains('['))
                {
                    finalNamespace.Add(line.Substring(0,  line.IndexOf('[') ));
                }
                else
                {
                    finalNamespace.Add(line);
                }
            }


            response = string.Join('.', finalNamespace);

            return response;
        }
        /// <summary>
        /// quits the first path of give json path exmp: "workorder.customer.data" = "customer.data"
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string WithoutPrefix(this string? str)
        {

            string response = string.Empty;

            var namespaces = str.Split('.').Skip(1).ToArray();

            response = string.Join('.', namespaces);
            return response;
        }

        /// <summary>
        /// Determines if the given JSON path contains the specified report path.
        /// </summary>
        /// <param name="jsonPath">The JSON path to search in.</param>
        /// <param name="reportPath">The report path to search for.</param>
        /// <returns>True if the JSON path contains the report path, otherwise false.</returns>
        public static bool HasPath(this string? jsonPath, string reportPath)
        {
            // Declare a string to store the response.
            string response = string.Empty;

            // Check if the report path contains a period.
            if (reportPath.Contains('.'))
            {
                // If so, check if the JSON path contains the report path with the wildcard characters removed.
                return jsonPath.Contains(reportPath.Replace("*", string.Empty));
            }
            else
            {
                // If not, check if the JSON path is equal to the report path.
                return jsonPath == reportPath;
            }

        }

        /// <summary>
        /// Removes script tags from given html string
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string StripHtmlTags(this string ? html)
        {
            return Regex.Replace(html, "<script[^>]*>[\\s\\S]*?</script>", string.Empty);
        }

        /// <summary>
        /// Encodes a given string to its base64
        /// </summary>
        /// <param name="plainText">Plain string to be encoded</param>
        /// <returns></returns>
        public static string Base64Encode(this string plainText) => Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));

        /// <summary>
        /// Decodes a given base64 string to its plain text
        /// </summary>
        /// <param name="base64String">Encoded string to be unencoded</param>
        /// <returns></returns>
        public static string Base64Decode(this string base64String) => Encoding.UTF8.GetString(Convert.FromBase64String(base64String));

        /// <summary>
        /// Get string stream
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Stream GetStream(this string s)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(s));
        }

        /// <summary>
        /// Checks if given word is plural or not
        /// </summary>
        /// <param name="s">The string</param>
        /// <returns>true if is plural word</returns>
        public static bool IsPluralWord(this string s) => PluralizationProvider.IsPlural(s);
    
        public static string ReplaceRange(this string wichString, string[] stringsToReplace, string replacement)
        {
            foreach(var s in stringsToReplace)
            {
                wichString = wichString.Replace(s, replacement);
            }

            return wichString;
        }

        public static string RemoveAllLetters(this string input)
        {
           
            return Regex.Replace(input, "[a-zA-Z]", "");
        }
    }
}
