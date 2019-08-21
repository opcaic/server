using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class SuperUserAuthorizationHandler : IAuthorizationHandler
	{
		/// <inheritdoc />
		public Task HandleAsync(AuthorizationHandlerContext context)
		{
			// admin has all rights in the platform.
			if (context.User.HasClaim(RolePolicy.UserRoleClaim, UserRole.Admin.ToString()))
			{
				foreach (var req in context.PendingRequirements.ToList())
				{
					context.Succeed(req);
				}
			}

			return Task.CompletedTask;
		}
	}
}