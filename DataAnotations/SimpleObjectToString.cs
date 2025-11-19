using System.ComponentModel.DataAnnotations;

namespace Ihelpers.DataAnotations
{
    /// <summary>
    /// The ObjectToString attribute is a validation attribute that always returns a success result.
    /// It is used to indicate that a property field should be serialized when an entity is going to be saved or deserialized when it is going to be shown in the frontend.
    /// It does not support array of objects.
    /// </summary>
    [Obsolete("Use ObjectToString attribute instead")]
    public class SimpleObjectToString : ValidationAttribute, IDataAnnotationBase
    {
        /// <summary>
        /// Returns a success result, indicating that the property should be serialized or deserialized as a string.
        /// </summary>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            return ValidationResult.Success;
        }
    }
}
