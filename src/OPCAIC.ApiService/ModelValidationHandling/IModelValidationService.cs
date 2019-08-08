using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.ModelValidationHandling
{
	public interface IModelValidationService
	{
		/// <summary>
		/// Tries to get a validation error with a given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns>Validation error or null</returns>
		ValidationErrorBase GetValidationError(string id);

        /// <summary>
		/// Processes a given validation error and adds it to a collection of errors with a generated id.
		/// This id is returned in the Message property of the validation result and is used later to
		/// retrieve the corresponding error.
		/// </summary>
		/// <param name="originalValidationResult"></param>
		/// <param name="error"></param>
		/// <returns></returns>
		ValidationResult ProcessValidationError(ValidationResult originalValidationResult, ValidationErrorBase error);
	}
}