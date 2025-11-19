using System.ComponentModel.DataAnnotations;

namespace Ihelpers.DataAnotations
{
    /// <summary>
    /// The RelationalField attribute is a validation attribute that always returns a success result.
    /// It is used to indicate that a property field is a relation and should be stored inside the base `dynamic_parameters` field when an entity is created or updated.
    /// When the entity is shown in the frontend, the relation field should be treated recursively using the camelCase conversion (Analyze) method in the `TransformerBase` class.
    /// </summary>
    public class RelationalField : ValidationAttribute, IDataAnnotationBase
    {
        /// <summary>
        /// Returns a success result, indicating that the property is a relation field and should be treated accordingly when creating, updating, or showing entities.
        /// </summary>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            return ValidationResult.Success;
        }
    }
}
