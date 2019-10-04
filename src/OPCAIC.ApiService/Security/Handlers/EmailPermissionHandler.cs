using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class EmailPermissionHandler : ResourcePermissionAuthorizationHandler<EmailPermission>
	{
		/// <inheritdoc />
		protected override Task<bool> HandlePermissionAsync(ClaimsPrincipal user, EmailPermission permission, long? id)
		{
			switch (permission)
			{
				case EmailPermission.Read:
					// only admin for now
					return Task.FromResult(false);
				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}