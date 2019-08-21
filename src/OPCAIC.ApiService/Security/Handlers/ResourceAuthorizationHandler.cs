using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace OPCAIC.ApiService.Security.Handlers
{
	public abstract class ResourcePermissionAuthorizationHandler<TPermission>
		: AuthorizationHandler<PermissionRequirement<TPermission>, long> where TPermission : Enum
	{
		/// <inheritdoc />
		protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
			PermissionRequirement<TPermission> requirement,
			long resourceId)
		{
			var idClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (idClaim == null ||
				!long.TryParse(idClaim, out var userId))
			{
				return; // cannot authorize
			}

			foreach (var permission in requirement.RequiredPermissions)
			{
				if (!await HandlePermissionAsync(userId, context.User, permission, resourceId))
				{
					return;
				}
			}

			context.Succeed(requirement);
		}

		/// <summary>
		///     Handles the authorization for the given permission.
		/// </summary>
		/// <param name="userId">Id of the current user to be authorized.</param>
		/// <param name="user">Instance of <see cref="ClaimsPrincipal"/> containing further claims.</param>
		/// <param name="permission">The permission to be verified.</param>
		/// <param name="resourceId">If the permission is instance-based, contains the id of the resource.</param>
		/// <returns></returns>
		protected abstract Task<bool> HandlePermissionAsync(long userId, ClaimsPrincipal user,
			TPermission permission,
			long resourceId);
	}
}
