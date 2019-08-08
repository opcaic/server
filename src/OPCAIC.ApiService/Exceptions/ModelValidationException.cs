using OPCAIC.ApiService.ModelValidationHandling;

namespace OPCAIC.ApiService.Exceptions
{
	public class ModelValidationException : ApiException
	{
		public ValidationErrorBase ValidationError { get; }

		/// <inheritdoc />
		public ModelValidationException(int statusCode, ValidationErrorBase validationError) : base(statusCode, validationError.Message)
		{
			ValidationError = validationError;
		}
	}
}