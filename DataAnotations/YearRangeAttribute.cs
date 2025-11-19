using System;
using System.ComponentModel.DataAnnotations;


namespace Ihelpers.DataAnotations
{
    public class YearRangeAttribute : ValidationAttribute
    {
        public int _minYear { get; }
        public int _maxYear { get; }
        public string? _label { get; } = null;

        private int _currentValue = 0;


        public YearRangeAttribute(int minYear, int maxYear, string? label)
        {
            _minYear = minYear;
            _maxYear = maxYear;
            _label = label;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime dateTimeValue)
            {
                if (dateTimeValue.Year < _minYear || dateTimeValue.Year > _maxYear)
                {
                    _currentValue = dateTimeValue.Year;
                    var errorMessage = FormatErrorMessage(validationContext.DisplayName);
                    return new ValidationResult(errorMessage, new[] { validationContext.DisplayName });
                }
            }
            return ValidationResult.Success;
        }

        public override string FormatErrorMessage(string name)
        {

            if (!string.IsNullOrEmpty(_label))
            {
                return $"The {_label} year must be between {_minYear} and {_maxYear}, you have entered {_currentValue}.";

            }
            else
            {

                return $"The {name} year must be between {_minYear} and {_maxYear}, you have entered {_currentValue}.";
            }
        }
    }
}
