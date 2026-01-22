using Ihelpers.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using System.Security.Policy;

namespace Ihelpers.Helpers
{
    public class ConfigurationHelper
    {
        public const string AZURE_FUNCTION_PATH = "/home/site/wwwroot";
        public static bool isAzureFunction = false;
        public const string ENVIRONMENT = "PLATFORM_ENV";
        /// <summary>
        /// Authentication options
        /// </summary>
        public PublicClientApplicationOptions PublicClientApplicationOptions { get; set; }

        /// <summary>
        /// Base URL for Microsoft Graph (it varies depending on whether the application is ran
        /// in Microsoft Azure public clouds or national / sovereign clouds
        /// </summary>
        public string MicrosoftGraphBaseEndpoint { get; set; }

        /// <summary>
        /// Reads the configuration from a json file
        /// </summary>
        /// <param name="path">Path to the configuration json file</param>
        /// <returns>SampleConfiguration as read from the json file</returns>
        public static ConfigurationHelper ReadFromJsonFile(string path)
        {
            // .NET configuration
            IConfigurationRoot Configuration;

            var builder = new ConfigurationBuilder()
             .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
             .AddJsonFile(path);

            Configuration = builder.Build();

            // Read the auth and graph endpoint config
            ConfigurationHelper config = new ConfigurationHelper()
            {
                PublicClientApplicationOptions = new PublicClientApplicationOptions()
            };
            Configuration.Bind("Authentication", config.PublicClientApplicationOptions);
            config.MicrosoftGraphBaseEndpoint = Configuration.GetValue<string>("WebAPI:MicrosoftGraphBaseEndpoint");
            return config;
        }

        /// <summary>
        /// Reads a configuration from a json file OR environment variables
        /// </summary>
        /// <param name="wichKey">The configuration key to retrieve</param>
        /// <param name="useCache">Whether to use cache</param>
        /// <returns>Configuration value as read from the json file or environment variables</returns>
        public static string? GetConfig(string wichKey = "EncryptKey:DefaultKey", bool useCache = false)
        {
            // .NET configuration

            if (useCache)
            {
                var cachedValue = ConfigContainer.cache.GetValue<string?>(wichKey).GetAwaiter().GetResult();
                if (cachedValue != null)
                {
                    return cachedValue;
                }
            }

            string? envValue = GetFromEnvironmentVariables(wichKey);
            Console.WriteLine($">>>{wichKey} VALUE IS {envValue}");

            if (!string.IsNullOrEmpty(envValue))
            {
                if (useCache)
                {
                    ConfigContainer.cache.CreateValue(wichKey, envValue, 60 * 60 * 240000);
                }
                return envValue;
            }
#if DEBUG

            string? environment = Environment.GetEnvironmentVariable(ENVIRONMENT) ?? "prod";
            Console.WriteLine($">>>THE ENVIRONMENT VALUE IS {environment}");

#endif 


            IConfigurationRoot configuration;
            if (isAzureFunction)
            {
#if DEBUG
                var configInternal = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

                if(!string.IsNullOrEmpty(environment))
                {
                    configInternal.AddJsonFile($"{Directory.GetCurrentDirectory()}/settings/appsettings.{environment}.json", optional: true);
                }
                configuration = configInternal.Build();


#else
            configuration = new ConfigurationBuilder()
                          .AddJsonFile("/home/site/wwwroot/appsettings.json")
                          .Build();
#endif
            }
            else
            {

#if DEBUG


                var configInternal = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

                if (!string.IsNullOrEmpty(environment))
                {
                    configInternal.AddJsonFile($"{Directory.GetCurrentDirectory()}/settings/appsettings.{environment}.json", optional: false);
                }
                configuration = configInternal.Build();

#else
configuration = new ConfigurationBuilder()
                       .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                       .AddJsonFile("appsettings.json")
                       .Build();
#endif

            }

            try
            {
                var configValue = configuration.GetValue(wichKey, "");

                if (useCache && !string.IsNullOrEmpty(configValue))
                {
                    ConfigContainer.cache.CreateValue(wichKey, configValue, 60 * 60 * 240000);
                }

                return configValue;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Reads a configuration from a json file OR environment variables and convert to desired type
        /// </summary>
        /// <param name="wichKey">The configuration key to retrieve</param>
        /// <param name="useCache">Whether to use cache</param>
        /// <returns>Configuration value as read from the json file or environment variables</returns>
        public static T? GetConfig<T>(string wichKey = "EncryptKey:DefaultKey", bool useCache = false)
        {
            // .NET configuration

            if (useCache)
            {
                var cachedValue = ConfigContainer.cache.GetValue<T>(wichKey).GetAwaiter().GetResult();
                if (cachedValue != null)
                {
                    return cachedValue;
                }
            }

            string? envValue = GetFromEnvironmentVariables(wichKey);
            Console.WriteLine($">>>{wichKey} VALUE IS {envValue}");

            if (!string.IsNullOrEmpty(envValue))
            {
                try
                {
                    var converter = TypeDescriptor.GetConverter(typeof(T));
                    T? convertedValue = (T?)converter.ConvertFromString(envValue);

                    if (useCache && convertedValue != null)
                    {
                        ConfigContainer.cache.CreateValue(wichKey, convertedValue, 60 * 60 * 240000);
                    }
                    return convertedValue;
                }
                catch
                {
                }
            }

            IConfigurationRoot configuration;
#if DEBUG

            string? environment = Environment.GetEnvironmentVariable(ENVIRONMENT);
#endif 
            if (isAzureFunction)
            {
#if DEBUG


                var configInternal = new ConfigurationBuilder()
                       .SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json");

                if (!string.IsNullOrEmpty(environment))
                {
                    configInternal.AddJsonFile($"{Directory.GetCurrentDirectory()}/settings/appsettings.{environment}.json", optional: true);
                }
                configuration = configInternal.Build();
#else
            configuration = new ConfigurationBuilder()
                          .AddJsonFile("/home/site/wwwroot/appsettings.json")
                          .Build();
#endif
            }
            else
            {
#if DEBUG


                var configInternal = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("appsettings.json");

                if (!string.IsNullOrEmpty(environment))
                {
                    configInternal.AddJsonFile($"{Directory.GetCurrentDirectory()}/settings/appsettings.{environment}.json", optional: false);
                }
                configuration = configInternal.Build();

#else
configuration = new ConfigurationBuilder()
                       .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                       .AddJsonFile("appsettings.json")
                       .Build();
#endif

            }

            try
            {
                T? configValue = configuration.GetSection(wichKey).Get<T>();

                if (useCache && configValue != null)
                {
                    ConfigContainer.cache.CreateValue(wichKey, configValue, 60 * 60 * 240000);
                }

                return configValue;
            }
            catch (Exception)
            {
                return default(T);
            }
        }

        /// <summary>
        /// Search a configuration in environment variables
        /// </summary>
        /// <param name="key">The configuration key</param>
        /// <returns> The stored value or null if not found </returns>
        private static string? GetFromEnvironmentVariables(string key)
        {
            try
            {
                //on linux env vars with ':' can have problems, so we try different formats
                string envVarName = key.Replace(":", "__");

                string? value = Environment.GetEnvironmentVariable(envVarName);
                Console.WriteLine($">>>{envVarName} THE ENV VALUE IS {value}");

                //If not found try with '.' instead of ':'
                if (string.IsNullOrEmpty(value))
                {
                    envVarName = key.Replace(':', '.');
                    value = Environment.GetEnvironmentVariable(envVarName);
                }

                // if still not found try with the original key
                if (string.IsNullOrEmpty(value))
                {
                    value = Environment.GetEnvironmentVariable(key);
                }

                return value;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

