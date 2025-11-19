using System.ComponentModel.DataAnnotations;

namespace Ihelpers.DataAnotations
{
    /// <summary>
    /// The Password attribute is a validation attribute that always returns a success result.
    /// It is used to indicate that a property field should be ignored when an entity is updated inside the IProfile.UserRepository, and replaced with **** when it is shown in the frontend.
    /// </summary>
    public class Password : ValidationAttribute, IDataAnnotationBase
    {
        /// <summary>
        /// Returns a success result, indicating that the password property should be ignored during updates and masked in the frontend.
        /// </summary>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            return ValidationResult.Success;
        }
    }
}
