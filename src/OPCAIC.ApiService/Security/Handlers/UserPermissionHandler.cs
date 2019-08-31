using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Extensions;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class ResourceIdAuth
	{
		/// <inheritdoc />
		public ResourceIdAuth(long id)
		{
			Id = id;
		}

		public long Id { get; }
	}

	public class UserPermissionHandler
		: ResourcePermissionAuthorizationHandler<UserPermission, ResourceIdAuth>
	{
		/// <inheritdoc />
		protected override Task<ResourceIdAuth> GetAuthorizationData(long resourceId,
			CancellationToken cancellationToken = default)
		{
			return Task.FromResult(new ResourceIdAuth(resourceId));
		}

		/// <inheritdoc />
		protected override bool HandlePermissionAsync(ClaimsPrincipal user,
			UserPermission permission,
			ResourceIdAuth authData)
		{
			switch (permission)
			{
				case UserPermission.Read:
				case UserPermission.Update:
					// user can do anything to themselves
					var userId = user.TryGetId();
					return userId == authData.Id;

				case UserPermission.Search:
					// no search for non-admin user's
					return false;
				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}