using System.Collections.Generic;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.Application.Infrastructure.Validation;

namespace OPCAIC.ApiService.Middlewares
{
	public class ValidationErrorModel
	{
		public string Title { get; set; } = "Invalid arguments to the API";
		public string Detail { get; set; } = "The inputs supplied to the API are invalid";
		public IEnumerable<ApplicationError> Errors { get; set; }
	}
}