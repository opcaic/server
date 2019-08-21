using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.ModelValidationHandling.Attributes
{
	public class ApiEmailAddressesAttribute : ApiValidationAttribute
	{
		/// <inheritdoc />
		protected override ValidationErrorBase GetValidationError(object value, ValidationContext validationContext)
		{
			var emailAddressAttribute = new EmailAddressAttribute();

			foreach (var emailAddress in (IEnumerable<string>) value)
			{
				var originalValidationResult =
					emailAddressAttribute.GetValidationResult(emailAddress, validationContext);

				if (originalValidationResult != ValidationResult.Success)
				{
					return new ValidationError(originalValidationResult);
				}
			}

			return null;
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
