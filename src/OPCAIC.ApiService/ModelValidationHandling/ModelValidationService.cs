using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.ModelValidationHandling
{
	public class ModelValidationService : IModelValidationService
	{
		private readonly Dictionary<string, ValidationErrorBase> errors =
			new Dictionary<string, ValidationErrorBase>();

		/// <inheritdoc />
		public ValidationErrorBase GetValidationError(string guid)
		{
			if (errors.TryGetValue(guid, out var error))
			{
				return error;
			}

			return null;
		}

		/// <inheritdoc />
		public ValidationResult ProcessValidationError(IEnumerable<string> memberNames,
			ValidationErrorBase error)
		{
			var guid = Guid.NewGuid().ToString();

			errors.Add(guid, error);

			return new ValidationResult(guid, memberNames);
		}
	}
}