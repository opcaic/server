using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Security;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Extensions
{
	public static class IdentityResultExtensions
	{
		public static void ThrowIfFailed(this IdentityResult result, string passwordFieldOverride = null, int statusCode = StatusCodes.Status400BadRequest)
		{
			Require.ArgNotNull(result, nameof(result));
			if (result.Succeeded != true)
			{
				throw new ModelValidationException(statusCode, result.GetValidationErrors(passwordFieldOverride));
			}
		}

		private static IEnumerable<AppIdentityErrorDescriber.IdentityValidationError> GetValidationErrors(this IdentityResult result, string passwordFieldOverride = null)
		{
			return result.Errors.Select(
				e =>
				{
					if (e is AppIdentityError aie)
					{
						if (aie.ValidationError.Field == nameof(NewUserModel.Password) &&
							passwordFieldOverride != null)
						{
							aie.ValidationError.Field = passwordFieldOverride;
						}
						return aie.ValidationError;
					}

					return new AppIdentityErrorDescriber.IdentityValidationError(
						ValidationErrorCodes.GenericError, null) {Message = e.Description};
				});
		}
	}
}