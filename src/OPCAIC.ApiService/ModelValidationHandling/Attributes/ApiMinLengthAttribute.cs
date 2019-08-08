using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace OPCAIC.ApiService.ModelValidationHandling.Attributes
{
	public class ApiMinLengthAttribute : MinLengthAttribute
	{
		public ApiMinLengthAttribute(int length) : base(length)
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

			var validationResult = errorHandlingService.ProcessValidationError(originalValidationResult, error);

			return validationResult;
		}

		private class ValidationError : ValidationErrorBase
		{
			public int Length { get; }

			public ValidationError(ValidationResult originalValidationResult, int length) : base(ValidationErrorCodes.MinLengthError, originalValidationResult)
			{
				Length = length;
			}
		}
	}
}