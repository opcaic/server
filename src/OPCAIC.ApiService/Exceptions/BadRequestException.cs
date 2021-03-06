﻿using Microsoft.AspNetCore.Http;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.Application.Infrastructure.Validation;

namespace OPCAIC.ApiService.Exceptions
{
	public class BadRequestException : ModelValidationException
	{
		public BadRequestException(string code, string message, string field)
			: base(StatusCodes.Status400BadRequest, new[] {new ValidationError(code, message, field)})
		{
		}
	}
}