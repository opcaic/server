using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace OPCAIC.ApiService.ModelValidationHandling.Attributes
{
	public class ApiRequiredAttribute : RequiredAttribute
	{
		protected override ValidationResult IsValid(object value,
			ValidationContext validationContext)
		{
			var originalValidationResult = base.IsValid(value, validationContext);

			if (originalValidationResult == ValidationResult.Success)
			{
				return ValidationResult.Success;
			}

			var errorHandlingService = validationContext.GetService<IModelValidationService>();
			var error = new ValidationError(originalValidationResult);

			var validationResult =
				errorHandlingService.ProcessValidationError(originalValidationResult.MemberNames, error);

			return validationResult;
		}

		private class ValidationError : ValidationErrorBase
		{
			public ValidationError(ValidationResult originalValidationResult) : base(
				ValidationErrorCodes.RequiredError, originalValidationResult)
			{
			}
		}
	}
}