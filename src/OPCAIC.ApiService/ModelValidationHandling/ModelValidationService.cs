using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OPCAIC.Application.Infrastructure.Validation;

namespace OPCAIC.ApiService.ModelValidationHandling
{
	public class ModelValidationService : IModelValidationService
	{
		private readonly Dictionary<string, ValidationError> errors =
			new Dictionary<string, ValidationError>();

		/// <inheritdoc />
		public ValidationError GetValidationError(string guid)
		{
			errors.TryGetValue(guid, out var error);
			return error;
		}

		/// <inheritdoc />
		public ValidationResult ProcessValidationError(IEnumerable<string> memberNames,
			ValidationError error)
		{
			var guid = Guid.NewGuid().ToString();

			errors.Add(guid, error);

			return new ValidationResult(guid, memberNames);
		}
	}
}