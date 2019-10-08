using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using FluentValidation.Validators;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Infrastructure;

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

		public static IRuleBuilderOptions<T, long> EntityId<T>(
			this IRuleBuilder<T, long> rule, Type entityType)
		{
			return rule.SetValidator((PropertyValidator) Activator.CreateInstance(typeof(EntityIdValidator<>).MakeGenericType(entityType)))
				.WithState((s, id) => new Dictionary<string, object> {["id"] = id})
				.WithErrorCode(ValidationErrorCodes.InvalidReference);
		}

		private class EntityIdValidator<TEntity> : PropertyValidator
			where TEntity : IEntity
		{
			/// <inheritdoc />
			public EntityIdValidator() 
				: base($"'{{PropertyName}}' does not reference valid {typeof(TEntity).Name}: {{PropertyValue}}.")
			{
			}

			/// <inheritdoc />
			protected override bool IsValid(PropertyValidatorContext context)
			{
				return IsValidAsync(context, CancellationToken.None).GetAwaiter().GetResult();
			}

			/// <inheritdoc />
			protected override Task<bool> IsValidAsync(PropertyValidatorContext context, CancellationToken cancellation)
			{
				var repository = context.GetServiceProvider().GetRequiredService<IRepository<TEntity>>();
				return repository.ExistsAsync((long)context.PropertyValue, cancellation);
			}
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

		public static IRuleBuilder<T, JObject> ValidGameConfigurationViaGameId<T>(
			this IRuleBuilder<T, JObject> rule, Func<T, long> gameIdGetter)
		{
			return rule.CustomAsync(async (o, context, token) =>
			{
				if (o == null)
					return;

				var schema = await context.GetServiceProvider().GetRequiredService<IRepository<Game>>()
					.GetAsync(t => t.Id == gameIdGetter((T)context.InstanceToValidate),
						t => t.ConfigurationSchema, token);

				ValidateGameConfiguration(context, o, schema);
			});
		}
		

		public static IRuleBuilder<T, JObject> ValidGameConfigurationViaTournamentId<T>(
			this IRuleBuilder<T, JObject> rule, Func<T, long> tournamentIdGetter)
		{
			return rule.CustomAsync(async (o, context, token) =>
			{
				if (o == null)
					return;

				var schema = await context.GetServiceProvider().GetRequiredService<IRepository<Tournament>>()
					.GetAsync(t => t.Id == tournamentIdGetter((T)context.InstanceToValidate),
						t => t.Game.ConfigurationSchema, token);

				ValidateGameConfiguration(context, o, schema);
			});
		}

		private static void ValidateGameConfiguration(CustomContext context, JObject config,
			string schema)
		{
			if (!config.IsValid(JSchema.Parse(schema), out IList<string> errors))
			{
				context.AddFailure(
					new ValidationFailure(context.PropertyName,
						"Configuration does not validate against game's schema")
					{
						ErrorCode = ValidationErrorCodes.InvalidSchema,
						CustomState = new Dictionary<string, object> { ["errors"] = errors }
					});
			}
		}

		public static IRuleBuilderOptions<T, string> IsEnumeration<T, TEnum>(
			this IRuleBuilder<T, string> rule)
			where TEnum : Enumeration<TEnum>, new()
		{
			return rule.Must(f => Enumeration<TEnum>.TryFromName(f, out _))
				.WithMessage("'{PropertyValue}' is not valid value for {PropertyName}.");
		}
	}
}