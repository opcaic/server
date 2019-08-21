using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class SubmissionPermissionHandler : ResourcePermissionAuthorizationHandler<SubmissionPermission>
	{
		/// <inheritdoc />
		protected override async Task<bool> HandlePermissionAsync(long userId, ClaimsPrincipal user, SubmissionPermission permission,
			long resourceId)
		{
			throw new NotImplementedException();
		}
	}
}