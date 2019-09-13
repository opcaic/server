using System.Collections.Generic;
using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace OPCAIC.ApiService.ModelValidationHandling
{
	public class ValidationInterceptor : IValidatorInterceptor
	{
		private readonly IModelValidationService validationService;

		public ValidationInterceptor(IModelValidationService validationService)
		{
			this.validationService = validationService;
		}

		/// <inheritdoc />
		public ValidationContext BeforeMvcValidation(ControllerContext controllerContext,
			ValidationContext validationContext)
		{
			return validationContext;
		}

		/// <inheritdoc />
		public ValidationResult AfterMvcValidation(ControllerContext controllerContext,
			ValidationContext validationContext, ValidationResult result)
		{
			if (!result.IsValid)
			{
				foreach (var error in result.Errors)
				{
					var newError =
						validationService.ProcessValidationError(new[] {error.PropertyName},
							new FluentValidationError(error.PropertyName,
								GetErrorCode(error.ErrorCode), error.ErrorMessage,
								(Dictionary<string, object>)error.CustomState));

					error.ErrorMessage = newError.ErrorMessage;
				}
			}

			return result;
		}

		private static string GetErrorCode(string fluentError)
		{
			// FluentValidation puts their own error codes beginning with uppercase, we want to
			// replace them by our generic error
			if (fluentError == null)
			{
				return ValidationErrorCodes.GenericError;
			}

			return char.IsLower(fluentError[0])
				? fluentError
				: ValidationErrorCodes.GenericError;
		}

		private class FluentValidationError : ValidationErrorBase
		{
			/// <inheritdoc />
			public FluentValidationError(string field, string code, string message,
				Dictionary<string, object> additionalData) : base(code, message)
			{
				Field = field;
				AdditionalData = additionalData;
			}

			[JsonExtensionData]
			public Dictionary<string, object> AdditionalData { get; }
		}
	}
}