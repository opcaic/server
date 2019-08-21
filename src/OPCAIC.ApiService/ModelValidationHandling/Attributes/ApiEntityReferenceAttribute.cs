using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.Infrastructure.DbContexts;

namespace OPCAIC.ApiService.ModelValidationHandling.Attributes
{
	public class ApiEntityReferenceAttribute : ApiValidationAttribute
	{
		public ApiEntityReferenceAttribute(Type entityType)
		{
			EntityType = entityType;
		}

		public Type EntityType { get; }

		/// <inheritdoc />
		protected override ValidationErrorBase GetValidationError(object value, ValidationContext validationContext)
		{
			if (value is long id)
			{
				// use db context directly to avoid having to create additional logic for each entity type.
				var dbContext = validationContext.GetRequiredService<DataContext>();
				if (dbContext.Find(EntityType, id) != null)
				{
					return null;
				}

				return new ValidationError(ValidationErrorCodes.InvalidReference, EntityType, id);
			}

			throw new InvalidOperationException("Wrong usage of attribute.");
		}

		private class ValidationError : ValidationErrorBase
		{
			/// <inheritdoc />
			public ValidationError(string code, Type type, long id) 
				: base(code, $"There is no entity of type {type.Name} with id {id}.")
			{
				Id = id;
			}

			public long Id { get; set; }

		}
	}
}