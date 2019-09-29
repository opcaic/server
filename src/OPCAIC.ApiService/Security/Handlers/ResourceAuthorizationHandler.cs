using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace OPCAIC.ApiService.Security.Handlers
{
	public abstract class ResourcePermissionAuthorizationHandler<TPermission>
		: AuthorizationHandler<PermissionRequirement<TPermission>, ResourceId>
		where TPermission : Enum
	{
		/// <inheritdoc />
		protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
			PermissionRequirement<TPermission> requirement,
			ResourceId resourceId)
		{
			if (await HandlePermissionAsync(context.User, requirement.RequiredPermission,
				resourceId.Id))
			{
				context.Succeed(requirement);
			}
		}

		/// <summary>
		///     Handles the authorization for the given permission.
		/// </summary>
		/// <param name="user">Instance of <see cref="ClaimsPrincipal" /> containing further claims.</param>
		/// <param name="permission">The permission to be verified.</param>
		/// <param name="id">
		///     If the permission is instance-based, contains the data needed for authorization decision.
		///     Otherwise null.
		/// </param>
		/// <returns></returns>
		protected abstract Task<bool> HandlePermissionAsync(ClaimsPrincipal user,
			TPermission permission,
			long? id);
	}
}