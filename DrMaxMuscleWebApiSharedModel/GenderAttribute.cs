using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrMaxMuscleWebApiSharedModel
{
    sealed public class GenderAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            string val = (string)value;
            switch (val)
            {
                case "Man":
                case "Woman":
                    return ValidationResult.Success;
                default:
                    return new ValidationResult(ErrorMessage);
            }
        }
    }
}
