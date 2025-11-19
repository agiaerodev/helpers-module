
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// This static class contains extension methods for registering options related to permission access inside the ControllerBase.
    /// </summary>
    public static class CoreControllerBaseEndpointsServiceCollectionExtensions
    {
        /// <summary>
        /// Enables permission access checking inside the ControllerBase.
        /// </summary>
        /// <param name="services">The service collection to which the options should be registered.</param>
        public static void CheckPermissionAccessInsideControllerBase(this IServiceCollection services)
        {
            Ihelpers.Extensions.ConfigContainer.CheckPermissionAccessInsideControllerBase = true;
        }

        /// <summary>
        /// Enables permission access checking inside the ControllerBase with specified options.
        /// </summary>
        /// <param name="services">The service collection to which the options should be registered.</param>
        /// <param name="options">An array of options for the permission access checking inside the ControllerBase.</param>
        public static void CheckPermissionAccessInsideControllerBase(this IServiceCollection services, params PermissionBaseOption[] options)
        {
            Ihelpers.Extensions.ConfigContainer.permissionBaseOptions = new();
            foreach (var option in options)
            {
                option.controller = option.controller.ToLower();
                Ihelpers.Extensions.ConfigContainer.CheckPermissionAccessInsideControllerBase = true;
                Ihelpers.Extensions.ConfigContainer.permissionBaseOptions.Add(option);
            }

        }
    }

    /// <summary>
    /// This class represents options for the permission access checking inside the ControllerBase.
    /// </summary>
    public class PermissionBaseOption
    {
        /// <summary>
        /// The name of the controller.
        /// </summary>
        public string controller { get; set; }

        /// <summary>
        /// The permission base for the controller.
        /// </summary>
        public string permissionBase { get; set; }

        /// <summary>
        /// The endpoint for the controller.
        /// </summary>
        public string endpoint { get; set; }

        /// <summary>
        /// The permission specific to the endpoint.
        /// </summary>
        public string endpointSpecificPermission { get; set; }

        /// <summary>
        /// A flag that indicates whether the permission access check should be ignored for the specified options.
        /// </summary>
        public bool ignore { get; set; } = false;
    }
}
