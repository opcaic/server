using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.ModelValidationHandling
{
	/// <summary>
	/// Represents a base for validation errors that are returned by the api service.
	/// </summary>
	public abstract class ValidationErrorBase
	{
		public string Code { get; }

		public string Message { get; set; }

		public string Field { get; set; }

		protected ValidationErrorBase(string code, ValidationResult originalValidationResult)
		{
			Code = code;
			Message = originalValidationResult.ErrorMessage;
		}

		protected ValidationErrorBase(string code, string message)
		{
			Code = code;
			Message = message;
		}
	}
}