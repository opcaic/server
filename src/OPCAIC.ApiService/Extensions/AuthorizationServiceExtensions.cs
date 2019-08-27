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
		private static async Task InternalAuthorizeAsync<TPermission>(
			IAuthorizationService authService, ClaimsPrincipal user, long? resourceId,
			params TPermission[] permissions)
			where TPermission : Enum
		{
			Debug.Assert(authService != null);
			var result = await authService.AuthorizeAsync(user, resourceId,
				new PermissionRequirement<TPermission>(permissions));

			if (!result.Succeeded)
			{
				throw new ForbiddenException("User is not authorized to perform given action.");
			}
		}

		public static Task CheckPermissions<TPermission>(
			this IAuthorizationService authService, ClaimsPrincipal user, long resourceId,
			params TPermission[] permissions)
			where TPermission : Enum
		{
			return InternalAuthorizeAsync(authService, user, resourceId, permissions);
		}

		public static Task CheckPermissions<TPermission>(
			this IAuthorizationService authService, ClaimsPrincipal user,
			params TPermission[] permissions)
			where TPermission : Enum
		{
			return InternalAuthorizeAsync(authService, user, null, permissions);
		}
	}
}