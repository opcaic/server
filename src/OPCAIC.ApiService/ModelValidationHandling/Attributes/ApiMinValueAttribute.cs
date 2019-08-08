using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;

namespace OPCAIC.ApiService.ModelValidationHandling.Attributes
{
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
	public class ApiMinValueAttribute : ValidationAttribute
	{
		public double MinValue { get; set; }

		public bool IsStrict { get; set; } = false;

		public ApiMinValueAttribute(double minValue)
		{
			this.MinValue = minValue;
		}

		public override bool IsValid(object value)
		{
			if (value == null)
				return true;

			if (value is IConvertible)
			{
				try
				{
					var doubleValue = Convert.ToDouble(value);
					return IsStrict ? doubleValue > MinValue : doubleValue >= MinValue;
				}
				catch (OverflowException)
				{
					return false;
				}
			}

			throw new InvalidOperationException("Wrong usage of attribute.");
		}

		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			var originalValidationResult = base.IsValid(value, validationContext);

			if (originalValidationResult == ValidationResult.Success)
			{
				return ValidationResult.Success;
			}

			var errorHandlingService = validationContext.GetService<IModelValidationService>();
			var error = new ValidationError(originalValidationResult, MinValue);

			var validationResult = errorHandlingService.ProcessValidationError(originalValidationResult, error);

			return validationResult;
		}

		public override string FormatErrorMessage(string name)
		{
			return $"The value of {name} is lower than {MinValue}.";
		}

		private class ValidationError : ValidationErrorBase
		{
			public double MinValue { get; }

			public ValidationError(ValidationResult originalValidationResult, double minValue) : base(ValidationErrorCodes.MinValueError, originalValidationResult)
			{
				MinValue = minValue;
			}
		}
	}
}
