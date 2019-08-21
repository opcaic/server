using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class UserPermissionHandler : ResourcePermissionAuthorizationHandler<UserPermission>
	{
		/// <inheritdoc />
		protected override Task<bool> HandlePermissionAsync(long userId, ClaimsPrincipal user, UserPermission permission,
			long resourceId)
		{
			switch (permission)
			{
				case UserPermission.Read:
				case UserPermission.Update:
					// user can do anything to themselves
					return Task.FromResult(userId == resourceId);
				case UserPermission.Search:
					// no search for non-admin user's
					return Task.FromResult(false);
				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}