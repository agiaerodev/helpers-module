using Ihelpers.Caching;
using Ihelpers.Interfaces;
using Ihelpers.Messages;
using Ihelpers.Messages.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Tracing;

namespace Ihelpers.Extensions
{
    /// <summary>
    /// This class serves as a container for the configuration options.
    /// </summary>
    public static class ConfigContainer
    {
        /// <summary>
        /// A flag that indicates whether permission access should be checked inside the ControllerBase.
        /// </summary>
        public static bool CheckPermissionAccessInsideControllerBase { get; set; } = false;

        /// <summary>
        /// A list of permission base options.
        /// </summary>
        public static List<PermissionBaseOption>? permissionBaseOptions { get; set; } = null;
         
        /// <summary>
        /// The cache instance, memory for clases storage.
        /// </summary>
        public static ICacheBase cache { get; set; } = new RawMemoryCache();


        /// <summary>
        /// The messaging instance.
        /// </summary>
        public static IMessageProvider messageProvider { get; set; } = null;


        public static void Configure(IServiceProvider serviceProvider)
        {
            messageProvider = serviceProvider.GetRequiredService<IMessageProvider>();

        }
    }
}
