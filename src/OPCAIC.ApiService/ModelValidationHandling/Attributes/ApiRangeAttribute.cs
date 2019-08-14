using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace OPCAIC.ApiService.ModelValidationHandling.Attributes
{
	public class ApiRangeAttribute : RangeAttribute
	{
		/// <inheritdoc />
		public ApiRangeAttribute(double minimum, double maximum) : base(minimum, maximum)
		{
		}

		/// <inheritdoc />
		public ApiRangeAttribute(int minimum, int maximum) : base(minimum, maximum)
		{
		}

		/// <inheritdoc />
		public ApiRangeAttribute(Type type, string minimum, string maximum) : base(type, minimum,
			maximum)
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
			var error = new ValidationError(originalValidationResult, Maximum, Minimum);

			var validationResult =
				errorHandlingService.ProcessValidationError(originalValidationResult, error);

			return validationResult;
		}

		private class ValidationError : ValidationErrorBase
		{
			public ValidationError(ValidationResult originalValidationResult, object minimum,
				object maximum) : base(ValidationErrorCodes.RangeError, originalValidationResult)
			{
				Minimum = minimum;
				Maximum = maximum;
			}

			public object Minimum { get; }

			public object Maximum { get; }
		}
	}
}