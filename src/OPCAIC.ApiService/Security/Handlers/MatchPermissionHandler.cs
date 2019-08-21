using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class MatchPermissionHandler : ResourcePermissionAuthorizationHandler<MatchPermission>
	{
		/// <inheritdoc />
		protected override async Task<bool> HandlePermissionAsync(long userId, ClaimsPrincipal user, MatchPermission permission,
			long resourceId)
		{
			throw new NotImplementedException();
		}
	}
}