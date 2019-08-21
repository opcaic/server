using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.ModelValidationHandling.Attributes
{
	public class ApiEmailAddressAttribute : ApiValidationAttribute
	{
		/// <inheritdoc />
		protected override ValidationErrorBase GetValidationError(object value, ValidationContext validationContext)
		{
			var emailAddressAttribute = new EmailAddressAttribute();
			var originalValidationResult =
				emailAddressAttribute.GetValidationResult(value, validationContext);

			return originalValidationResult == ValidationResult.Success 
				? null 
				: new ValidationError(originalValidationResult);
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