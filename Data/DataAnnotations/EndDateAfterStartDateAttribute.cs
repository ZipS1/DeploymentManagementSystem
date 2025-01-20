using System.ComponentModel.DataAnnotations;

namespace DeploymentManagementSystem.Data.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class EndDateAfterStartDateAttribute : ValidationAttribute
    {
        private readonly string _startDatePropertyName;
        private readonly string _endDatePropertyName;

        public EndDateAfterStartDateAttribute(string startDatePropertyName, string endDatePropertyName)
        {
            _startDatePropertyName = startDatePropertyName;
            _endDatePropertyName = endDatePropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var startDateProp = validationContext.ObjectType.GetProperty(_startDatePropertyName);
            var endDateProp = validationContext.ObjectType.GetProperty(_endDatePropertyName);

            if (startDateProp == null)
            {
                return new ValidationResult($"Unknown property: {_startDatePropertyName}");
            }
            if (endDateProp == null)
            {
                return new ValidationResult($"Unknown property: {_endDatePropertyName}");
            }

            var startDateValue = (DateTime?)startDateProp.GetValue(validationContext.ObjectInstance);
            var endDateValue = (DateTime?)endDateProp.GetValue(validationContext.ObjectInstance);

            if (endDateValue.HasValue && startDateValue.HasValue)
            {
                if (endDateValue.Value < startDateValue.Value)
                {
                    return new ValidationResult(ErrorMessage ?? $"{_endDatePropertyName} должно быть позже {_startDatePropertyName}.");
                }
            }

            return ValidationResult.Success;
        }
    }
}
