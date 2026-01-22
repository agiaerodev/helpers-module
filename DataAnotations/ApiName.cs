using System;
using System.ComponentModel.DataAnnotations;


namespace Ihelpers.DataAnotations
{
    public class ApiName : ValidationAttribute, IDataAnnotationBase
    {
        public string? _label { get; } = null;



        public ApiName( string? label)
        {

            _label = label;
        }

        /// <summary>
        /// Returns a success result, indicating that the validation should not be converted to the user's timezone.
        /// </summary>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            return ValidationResult.Success;
        }
    }

}
