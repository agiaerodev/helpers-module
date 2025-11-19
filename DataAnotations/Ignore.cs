using System.ComponentModel.DataAnnotations;

namespace Ihelpers.DataAnotations
{
    /// <summary>
    /// The Ignore attribute is a validation attribute that always returns a success result.
    /// It is used to ignore a specific property when performing validation inside transformerBase class.
    /// </summary>
    public class Ignore : ValidationAttribute, IDataAnnotationBase
    {
        /// <summary>
        /// Returns a success result, indicating that the validation should be ignored.
        /// </summary>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            return ValidationResult.Success;
        }
    }
}
