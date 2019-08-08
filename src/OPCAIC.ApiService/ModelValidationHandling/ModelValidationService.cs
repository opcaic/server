using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.ModelValidationHandling
{
	public class ModelValidationService : IModelValidationService
	{
		private readonly Dictionary<Guid, ValidationErrorBase> errors = new Dictionary<Guid, ValidationErrorBase>();

		/// <inheritdoc />
		public ValidationErrorBase GetValidationError(string id)
		{
			if (!Guid.TryParse(id, out var guid))
			{
				return null;
			}

			if (errors.TryGetValue(guid, out var error))
			{
				return error;
			}

			return null;
		}

		/// <inheritdoc />
		public ValidationResult ProcessValidationError(ValidationResult originalValidationResult, ValidationErrorBase error)
		{
			var guid = Guid.NewGuid();

			errors.Add(guid, error);

			return new ValidationResult(guid.ToString(), originalValidationResult.MemberNames);
		}
	}
}