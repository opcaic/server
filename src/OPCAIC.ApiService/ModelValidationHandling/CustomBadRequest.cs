using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Utils;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Validation;

namespace OPCAIC.ApiService.ModelValidationHandling
{
	public class CustomBadRequest
	{
		private readonly IModelValidationService modelValidationService;

		public CustomBadRequest(ActionContext context,
			IModelValidationService modelValidationService)
		{
			this.modelValidationService = modelValidationService;
			Title = "Invalid arguments to the API";
			Detail = "The inputs supplied to the API are invalid";
			ConstructErrorMessages(context);
		}

		public List<ApplicationError> Errors { get; set; } = new List<ApplicationError>();

		public string Title { get; }

		public string Detail { get; }

		private void ConstructErrorMessages(ActionContext context)
		{
			foreach (var (key, value) in context.ModelState)
			{
				var field = key.FirstLetterToLower();
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
							Errors.Add(new ValidationError(ValidationErrorCodes.GenericError,
								message, field));
						}
					}
				}
			}
		}
	}
}