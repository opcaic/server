using System;
using System.Collections.Generic;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Persistence;

namespace OPCAIC.ApiService.ModelValidationHandling
{
	public static class RuleBuilderExtensions
	{
		// cannot move these to Application project yet
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