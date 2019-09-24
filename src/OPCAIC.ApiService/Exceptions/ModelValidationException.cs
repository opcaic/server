using System.Collections.Generic;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.Application.Infrastructure.Validation;

namespace OPCAIC.ApiService.Exceptions
{
	public class ModelValidationException : ApiException
	{
		/// <inheritdoc />
		public ModelValidationException(int statusCode,
			IEnumerable<ApplicationError> validationErrors) : base(
			statusCode, nameof(ModelValidationException), null)
		{
			ValidationErrors = validationErrors;
		}

		public IEnumerable<ApplicationError> ValidationErrors { get; }
	}
}