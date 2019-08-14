using System;
using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.Validation
{
	[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property)]
	public class MinValueAttribute : ValidationAttribute
	{
		private readonly double fMinValue;

		public MinValueAttribute(double minValue)
		{
			fMinValue = minValue;
		}

		public bool IsStrict { get; set; } = false;

		public override bool IsValid(object value)
		{
			if (value == null)
			{
				return true;
			}

			if (value is IConvertible)
			{
				try
				{
					var doubleValue = Convert.ToDouble(value);
					return IsStrict ? doubleValue > fMinValue : doubleValue >= fMinValue;
				}
				catch (OverflowException)
				{
					return false;
				}
			}

			throw new InvalidOperationException("Wrong usage of attribute.");
		}

		public override string FormatErrorMessage(string name)
		{
			return $"The value of {name} is lower than {fMinValue}.";
		}
	}
}