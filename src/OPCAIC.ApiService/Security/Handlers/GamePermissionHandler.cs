using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class GamePermissionHandler
		: ResourcePermissionAuthorizationHandler<GamePermission>
	{
		/// <inheritdoc />
		protected override Task<bool> HandlePermissionAsync(ClaimsPrincipal user,
			GamePermission permission,
			long? id)
		{
			switch (permission)
			{
				case GamePermission.Create:
				case GamePermission.Update:
				case GamePermission.Delete:
					return Task.FromResult(false); // only admin, and he has his own handler

				case GamePermission.Read:
				case GamePermission.Search:
					return
						Task.FromResult(
							true); // TODO: Verify, or maybe more granular level (details or not)
				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}