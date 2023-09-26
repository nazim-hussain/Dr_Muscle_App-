using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Reflection;

namespace DrMaxMuscleWebApiSharedModel
{
    sealed public class DateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime val = (DateTime)value;
            if (val == DateTime.MinValue)
                return new ValidationResult(ErrorMessage);

            return ValidationResult.Success;
        }
    }
}
