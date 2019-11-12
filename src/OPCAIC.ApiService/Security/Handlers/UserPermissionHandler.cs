using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Extensions;
using OPCAIC.Domain.Enums;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class UserPermissionHandler
		: ResourcePermissionAuthorizationHandler<UserPermission>
	{
		/// <inheritdoc />
		protected override Task<bool> HandlePermissionAsync(ClaimsPrincipal user,
			UserPermission permission, long? id)
		{
			switch (permission)
			{
				case UserPermission.Read:
				case UserPermission.Update:
					// user can do anything to themselves
					var userId = user.TryGetId();
					return Task.FromResult(userId == id.Value);

				case UserPermission.Search:
					// only admin and organizers, admin has his own handler
					return Task.FromResult(user.GetUserRole() == UserRole.Organizer);
				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}