using System.Collections.Generic;
using FluentValidation.Results;
using FluentValidation.Validators;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.ModelValidationHandling
{
	public static class CustomContextExtensions
	{
		public static void AddFailure(this CustomContext context, string propertyName, string message, string errorCode)
		{
			Require.ArgNotNull(context, nameof(context));

			context.AddFailure(new ValidationFailure(propertyName, message)
			{
				ErrorCode = errorCode
			});
		}
		public static void AddFailure(this CustomContext context, string propertyName, string message, string errorCode, (string name, object value) additionalData)
		{
			Require.ArgNotNull(context, nameof(context));

			context.AddFailure(new ValidationFailure(propertyName, message)
			{
				ErrorCode = errorCode,
				CustomState = new Dictionary<string, object>
				{
					[additionalData.name] = additionalData.value
				}
			});
		}
	}
}