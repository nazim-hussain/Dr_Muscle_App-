using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Reflection;

namespace DrMaxMuscleWebApiSharedModel
{
    sealed public class CompareAttribute : ValidationAttribute
    {
        private string _property = "";

        public CompareAttribute(string property)
        {

            _property = property;
        }
        
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = ((PropertyInfo) validationContext.ObjectType.GetRuntimeProperty(_property));
            if (property == null)
            {
                return new ValidationResult(
                    string.Format("Unknown property: {0}", _property)
                );
            }
            var otherValue = property.GetValue(validationContext.ObjectInstance, null);
            if ((string)value == (string)otherValue)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(ErrorMessage);
        }
    }
}
