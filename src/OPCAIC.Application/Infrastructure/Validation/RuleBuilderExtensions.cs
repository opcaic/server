using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FluentValidation;

namespace OPCAIC.Application.Infrastructure.Validation
{
	public static class RuleBuilderExtensions
	{
		public static IRuleBuilderOptions<T, TProperty> Required<T, TProperty>(
			this IRuleBuilder<T, TProperty> rule)
		{
			return rule.NotNull()
				.WithErrorCode(ValidationErrorCodes.RequiredError);
		}

		public static IRuleBuilderOptions<T, int> InRange<T>(
			this IRuleBuilder<T, int> rule, int min, int max)
		{
			var dict = new Dictionary<string, object> { ["minimum"] = min, ["maximum"] = max };
			return rule.InclusiveBetween(min, max)
				.WithState(_ => dict)
				.WithErrorCode(ValidationErrorCodes.RangeError);
		}

		public static IRuleBuilderOptions<T, TVal?> InRange<T, TVal>(
			this IRuleBuilder<T, TVal?> rule, TVal min, TVal max)
			where TVal: struct, IComparable, IComparable<TVal>
		{
			var dict = new Dictionary<string, object> { ["minimum"] = min, ["maximum"] = max };
			return rule.InclusiveBetween(min, max)
				.WithState(_ => dict)
				.WithErrorCode(ValidationErrorCodes.RangeError);
		}

		public static IRuleBuilderOptions<T, string> Email<T>(
			this IRuleBuilder<T, string> rule)
		{
			return rule.EmailAddress()
				.WithMessage("'{PropertyValue}' is not a valid email address.")
				.WithErrorCode(ValidationErrorCodes.InvalidEmailError);
		}

		public static IRuleBuilderOptions<T, string> MaxLength<T>(
			this IRuleBuilder<T, string> rule, int length)
		{
			var dict = new Dictionary<string, object> {["length"] = length};
			return rule.MaximumLength(length)
				.WithState(_ => dict)
				.WithErrorCode(ValidationErrorCodes.MaxLengthError);
		}

		public static IRuleBuilderOptions<T, string> MinLength<T>(
			this IRuleBuilder<T, string> rule, int length)
		{
			var dict = new Dictionary<string, object> {["length"] = length};
			return rule.MinimumLength(length)
				.WithState(_ => dict)
				.WithErrorCode(ValidationErrorCodes.MinLengthError);
		}

		public static IRuleBuilderOptions<T, TProperty> MinValue<T, TProperty>(
			this IRuleBuilder<T, TProperty> rule, TProperty minimum)
			where TProperty : IComparable<TProperty>, IComparable
		{
			var dict = new Dictionary<string, object> {["minValue"] = minimum};
			return rule.GreaterThanOrEqualTo(_ => minimum)
				.WithState(_ => dict)
				.WithErrorCode(ValidationErrorCodes.MinValueError);
		}

		public static IRuleBuilderOptions<T, string> Url<T>(
			this IRuleBuilder<T, string> rule)
		{
			return rule
				.Must(value => value == null || Uri.IsWellFormedUriString(value, UriKind.Absolute))
				.WithErrorCode(ValidationErrorCodes.InvalidUrlError);
		}
	}
}