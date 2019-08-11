using System.Collections.Generic;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc;

namespace OPCAIC.ApiService.ModelValidationHandling
{
	public class CustomBadRequest
	{
		private readonly IModelValidationService modelValidationService;

		public List<ValidationErrorBase> Errors { get; set; } = new List<ValidationErrorBase>();

		public string Title { get; }

		public string Detail { get; }

		public CustomBadRequest(ActionContext context, IModelValidationService modelValidationService)
		{
			this.modelValidationService = modelValidationService;
			Title = "Invalid arguments to the API";
			Detail = "The inputs supplied to the API are invalid";
			ConstructErrorMessages(context);
		}

		private void ConstructErrorMessages(ActionContext context)
		{
			foreach (var (key, value) in context.ModelState)
			{
				var field = key.IsNullOrEmpty() ? key : char.ToLowerInvariant(key[0]) + key.Substring(1);
				var errors = value.Errors;

				if (errors != null && errors.Count > 0)
				{
					foreach (var modelError in errors)
					{
						var message = modelError.ErrorMessage;
						var error = modelValidationService.GetValidationError(message);

						if (error != null)
						{
							error.Field = field;
							Errors.Add(error);
						}
						else
						{
							Errors.Add(new ValidationError(ValidationErrorCodes.GenericError, message, field));
						}
					}
				}
			}
		}
	}
}