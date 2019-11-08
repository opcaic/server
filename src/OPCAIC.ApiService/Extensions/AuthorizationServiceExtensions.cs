using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Security;

namespace OPCAIC.ApiService.Extensions
{
	public static class AuthorizationServiceExtensions
	{
		private static async Task<bool> InternalHasPermission<TPermission>(
			IAuthorizationService authService, ClaimsPrincipal user, ResourceId resourceId,
			TPermission permission)
			where TPermission : Enum
		{
			Debug.Assert(authService != null);
			Debug.Assert(resourceId != null);
			Debug.Assert(user != null);

			var result = await authService.AuthorizeAsync(user, resourceId,
				new PermissionRequirement<TPermission>(permission));

			return result.Succeeded;
		}
		private static async Task InternalAuthorizeAsync<TPermission>(
			IAuthorizationService authService, ClaimsPrincipal user, ResourceId resourceId,
			TPermission permission)
			where TPermission : Enum
		{
			if (!await InternalHasPermission(authService, user, resourceId, permission))
			{
				throw new ForbiddenException("User is not authorized to perform given action.");
			}
		}

		public static Task<bool> HasPermission<TPermission>(this IAuthorizationService authService, ClaimsPrincipal user, long resourceId,
			TPermission permission)
			where TPermission : Enum
		{
			return InternalHasPermission(authService, user, new ResourceId(resourceId), permission);
		}

		public static Task CheckPermission<TPermission>(
			this IAuthorizationService authService, ClaimsPrincipal user, long resourceId,
			TPermission permission)
			where TPermission : Enum
		{
			return InternalAuthorizeAsync(authService, user, new ResourceId(resourceId), permission);
		}

		public static Task CheckPermission<TPermission>(
			this IAuthorizationService authService, ClaimsPrincipal user,
			TPermission permission)
			where TPermission : Enum
		{
			return InternalAuthorizeAsync(authService, user, ResourceId.Null, permission);
		}
	}
}