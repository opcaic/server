using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace OPCAIC.ApiService.ModelValidationHandling.Attributes
{
	public abstract class ApiValidationAttribute : ValidationAttribute
	{
		protected sealed override ValidationResult IsValid(object value,
			ValidationContext validationContext)
		{
			var errorHandlingService = validationContext.GetService<IModelValidationService>();
			var error = GetValidationError(value, validationContext);

			if (error == null)
			{
				return ValidationResult.Success;
			}

			var validationResult =
				errorHandlingService.ProcessValidationError(new[] {validationContext.MemberName}, error);

			return validationResult;
		}

		protected ValidationResult BaseIsValid(object value, ValidationContext validationContext)
		{
			return base.IsValid(value, validationContext);
		}

		protected abstract ValidationErrorBase GetValidationError(object value, ValidationContext validationContext);
	}
}