using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class UserAuthDto
	{
		public long Id { get; set; }
	}

	public class UserPermissionHandler
		: ResourcePermissionAuthorizationHandler<UserPermission, UserAuthDto>
	{
		/// <inheritdoc />
		protected override Task<UserAuthDto> GetAuthorizationData(long resourceId,
			CancellationToken cancellationToken = default)
		{
			return Task.FromResult(new UserAuthDto {Id = resourceId});
		}

		/// <inheritdoc />
		protected override bool HandlePermissionAsync(long userId, ClaimsPrincipal user,
			UserPermission permission,
			UserAuthDto resourceId)
		{
			switch (permission)
			{
				case UserPermission.Read:
				case UserPermission.Update:
					// user can do anything to themselves
					return userId == resourceId.Id;
				case UserPermission.Search:
					// no search for non-admin user's
					return false;
				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}