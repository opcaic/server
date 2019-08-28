using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Security;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Extensions
{
	public static class IdentityResultExtensions
	{
		public static void ThrowIfFailed(this IdentityResult result, int statusCode = StatusCodes.Status500InternalServerError)
		{
			Require.ArgNotNull(result, nameof(result));
			if (result.Succeeded != true)
			{
				throw new ModelValidationException(statusCode, result.Errors.Select(
					e =>
					{
						if (e is AppIdentityError aie)
						{
							return aie.ValidationError;
						}

						return new AppIdentityErrorDescriber.IdentityValidationError(
							ValidationErrorCodes.GenericError, null) {Message = e.Description};
					}));
			}
		}
	}
}