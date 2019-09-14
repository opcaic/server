using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.DbContexts;

namespace OPCAIC.ApiService.ModelValidationHandling
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
			var dict = new Dictionary<string, object> {["minimum"] = min, ["maximum"] = max};
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

		public static IRuleBuilderOptions<T, long> EntityId<T>(
			this IRuleBuilder<T, long> rule, Type entityType)
		{
			return rule.MustAsync(async (_, id, ctx, cancellationToken) =>
					await ctx.GetServiceProvider().GetRequiredService<DataContext>()
						.FindAsync(entityType, new object[] {id}, cancellationToken) !=
					null
				).WithMessage("'{PropertyName}' does not reference valid entity: {PropertyValue}.")
				.WithState((s, id) => new Dictionary<string, object> {["id"] = id})
				.WithErrorCode(ValidationErrorCodes.InvalidReference);
		}

		public static IRuleBuilder<T, JObject> ValidSchema<T>(
			this IRuleBuilder<T, JObject> rule)
		{
			return rule.Custom((s, context) =>
			{
				if (s != null && !s.IsValid(JsonSchemaDefinition.Version7,
					out IList<string> errors))
				{
					context.AddFailure(
						new ValidationFailure(context.PropertyName,
							"Specified JSON Schema is not valid.")
						{
							ErrorCode = ValidationErrorCodes.InvalidSchema,
							CustomState = new Dictionary<string, object> {["errors"] = errors}
						});
				}
			});
		}
	}
}