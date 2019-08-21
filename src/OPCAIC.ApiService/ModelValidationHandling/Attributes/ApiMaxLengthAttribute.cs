using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace OPCAIC.ApiService.ModelValidationHandling.Attributes
{
	public class ApiMaxLengthAttribute : MaxLengthAttribute
	{
		public ApiMaxLengthAttribute(int length) : base(length)
		{
		}

		protected override ValidationResult IsValid(object value,
			ValidationContext validationContext)
		{
			var originalValidationResult = base.IsValid(value, validationContext);

			if (originalValidationResult == ValidationResult.Success)
			{
				return ValidationResult.Success;
			}

			var errorHandlingService = validationContext.GetService<IModelValidationService>();
			var error = new ValidationError(originalValidationResult, Length);

			var validationResult =
				errorHandlingService.ProcessValidationError(originalValidationResult.MemberNames, error);

			return validationResult;
		}

		private class ValidationError : ValidationErrorBase
		{
			public ValidationError(ValidationResult originalValidationResult, int length) : base(
				ValidationErrorCodes.MaxLengthError, originalValidationResult)
			{
				Length = length;
			}

			public int Length { get; }
		}
	}
}