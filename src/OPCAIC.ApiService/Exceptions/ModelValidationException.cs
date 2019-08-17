﻿using System.Collections.Generic;
using OPCAIC.ApiService.ModelValidationHandling;

namespace OPCAIC.ApiService.Exceptions
{
	public class ModelValidationException : ApiException
	{
		/// <inheritdoc />
		public ModelValidationException(int statusCode, IEnumerable<ValidationErrorBase> validationErrors) : base(
			statusCode, "Invalid model.")
		{
			ValidationErrors = validationErrors;
		}

		public IEnumerable<ValidationErrorBase> ValidationErrors { get; }
	}
}