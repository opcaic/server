using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace OPCAIC.ApiService.ModelValidationHandling.Attributes
{
	public class ApiEmailAddressesAttribute : ValidationAttribute
	{
		protected override ValidationResult IsValid(object value,
			ValidationContext validationContext)
		{
			var emailAddressAttribute = new EmailAddressAttribute();

			var emailAddresses = value as IEnumerable<string>;

			foreach (var emailAddress in emailAddresses)
			{
				var originalValidationResult =
					emailAddressAttribute.GetValidationResult(emailAddress, validationContext);

				if (originalValidationResult != ValidationResult.Success)
				{

					var errorHandlingService = validationContext.GetService<IModelValidationService>();
					var error = new ValidationError(originalValidationResult);

					var validationResult =
						errorHandlingService.ProcessValidationError(originalValidationResult, error);

					return validationResult;
				}
			}
			return ValidationResult.Success;
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
