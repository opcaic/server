using Microsoft.AspNetCore.Http;
using OPCAIC.ApiService.ModelValidationHandling;

namespace OPCAIC.ApiService.Exceptions
{
	public class ConflictException : ModelValidationException
	{
		public ConflictException(ValidationErrorBase validationError)
			: base(StatusCodes.Status409Conflict, new[] {validationError})
		{
		}

		public ConflictException(string code, string message, string field)
			: this(new ValidationError(code, message ?? nameof(ConflictException), field))
		{
		}
	}
}