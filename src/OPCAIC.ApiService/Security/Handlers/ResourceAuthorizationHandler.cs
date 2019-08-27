using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OPCAIC.ApiService.Exceptions;

namespace OPCAIC.ApiService.Security.Handlers
{
	public abstract class ResourcePermissionAuthorizationHandler<TPermission, TAuthData>
		: AuthorizationHandler<PermissionRequirement<TPermission>, long?>
		where TPermission : Enum
		where TAuthData : class
	{
		/// <inheritdoc />
		protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
			PermissionRequirement<TPermission> requirement,
			long? resourceId)
		{
			var idClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (idClaim == null ||
				!long.TryParse(idClaim, out var userId))
			{
				return; // cannot authorize user without id
			}

			TAuthData authData = default;
			if (resourceId.HasValue)
			{
				authData = await GetAuthorizationData(resourceId.Value);

				if (authData == default(TAuthData))
				{
					throw new NotFoundException(typeof(TPermission).Name.Replace("Permission", ""),
						resourceId.Value);
				}
			}

			foreach (var permission in requirement.RequiredPermissions)
			{
				if (!HandlePermissionAsync(userId, context.User, permission, authData))
				{
					return;
				}
			}

			context.Succeed(requirement);
		}

		protected abstract Task<TAuthData> GetAuthorizationData(long resourceId,
			CancellationToken cancellationToken = default);

		/// <summary>
		///     Handles the authorization for the given permission.
		/// </summary>
		/// <param name="userId">Id of the current user to be authorized.</param>
		/// <param name="user">Instance of <see cref="ClaimsPrincipal" /> containing further claims.</param>
		/// <param name="permission">The permission to be verified.</param>
		/// <param name="authData">
		///     If the permission is instance-based, contains the data needed for authorization decision.
		///     Otherwise null.
		/// </param>
		/// <returns></returns>
		protected abstract bool HandlePermissionAsync(long userId, ClaimsPrincipal user,
			TPermission permission,
			TAuthData authData);
	}
}