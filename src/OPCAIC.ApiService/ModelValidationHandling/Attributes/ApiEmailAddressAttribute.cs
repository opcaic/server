using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace OPCAIC.ApiService.ModelValidationHandling.Attributes
{
	public class ApiEmailAddressAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value,
			ValidationContext validationContext)
		{
			var emailAddressAttribute = new EmailAddressAttribute();
			var originalValidationResult = emailAddressAttribute.GetValidationResult(value, validationContext);

			if (originalValidationResult == ValidationResult.Success)
			{
				return ValidationResult.Success;
			}

			var errorHandlingService = validationContext.GetService<IModelValidationService>();
			var error = new ValidationError(originalValidationResult);

			var validationResult =
				errorHandlingService.ProcessValidationError(originalValidationResult, error);

			return validationResult;
		}

		private class ValidationError : ValidationErrorBase
		{
			public ValidationError(ValidationResult originalValidationResult) : base(
				ValidationErrorCodes.InvalidEmailError, originalValidationResult)
			{
			}
		}
	}
}