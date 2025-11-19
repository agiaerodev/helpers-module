using Ihelpers.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System.Reflection;
using System.Security.Policy;

namespace Ihelpers.Helpers
{
    public class ConfigurationHelper
    {

        public const string AZURE_FUNCTION_PATH = "/home/site/wwwroot";

        public static bool isAzureFunction = false;
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
        /// Reads a  configuration from a json file
        /// </summary>
        /// <param name="path">Path to the configuration json file</param>
        /// <returns>SampleConfiguration as read from the json file</returns>
        public static string? GetConfig(string wichKey = "EncryptKey:DefaultKey", bool useCache = false)
        {
            // .NET configuration
            // .NET configuration


            if (useCache)
            {
                var cachedValue = ConfigContainer.cache.GetValue<string?>(wichKey).GetAwaiter().GetResult();

                if (cachedValue != null)
                {
                    return cachedValue;
                }
            }

            IConfigurationRoot configuration;
            if (isAzureFunction)
            {
#if DEBUG
                               configuration = new ConfigurationBuilder()
                              .SetBasePath(Directory.GetCurrentDirectory())
                              .AddJsonFile("appsettings.json")
                              .AddEnvironmentVariables()
                              .Build();
#else
                configuration = new ConfigurationBuilder()
                          .AddJsonFile("/home/site/wwwroot/appsettings.json")
                          .Build();
#endif

            }
            else
            {
                configuration = new ConfigurationBuilder()
               .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
               .AddJsonFile("appsettings.json")
               .AddEnvironmentVariables()
               .Build();
            }

            try
            {
                var configValue = configuration.GetValue(wichKey, "");

                if (useCache)
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
        /// Reads a configuration from a json file and convert to desired type
        /// </summary>
        /// <param name="path">Path to the configuration json file</param>
        /// <returns>SampleConfiguration as read from the json file</returns>
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

            IConfigurationRoot configuration;

            if (isAzureFunction)
            {
                #if DEBUG
                               configuration = new ConfigurationBuilder()
                              .SetBasePath(Directory.GetCurrentDirectory())
                              .AddJsonFile("appsettings.json")
                              .AddEnvironmentVariables()
                              .Build();
#else
                              configuration = new ConfigurationBuilder()
                                        .AddJsonFile("/home/site/wwwroot/appsettings.json")
                                        .Build();
#endif

            }
            else
            {
                configuration = new ConfigurationBuilder()
               .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
               .AddJsonFile("appsettings.json")
               .AddEnvironmentVariables()
               .Build();
            }


            // .NET configuration

            try
            {
                T? configValue = configuration.GetSection(wichKey).Get<T>();

                if (useCache)
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

    }
}
