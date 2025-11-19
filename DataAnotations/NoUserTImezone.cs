using System.ComponentModel.DataAnnotations;

namespace Ihelpers.DataAnotations
{
    /// <summary>
    /// The NoUserTimezone attribute is a validation attribute that always returns a success result.
    /// It is used to indicate that a `DateTime` field should not be converted to the requesting user's timezone.
    /// </summary>
    public class NoUserTimezone : ValidationAttribute, IDataAnnotationBase
    {
        /// <summary>
        /// Returns a success result, indicating that the validation should not be converted to the user's timezone.
        /// </summary>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            return ValidationResult.Success;
        }
    }
}
