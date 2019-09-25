using Microsoft.AspNetCore.Authorization;
using OPCAIC.Domain.Enums;

namespace OPCAIC.ApiService.Security
{
	public static class AuthorizationConfiguration
	{
		public static void Setup(AuthorizationOptions options)
		{
			AddPolicy(options, RolePolicy.Admin, nameof(UserRole.Admin));
			AddPolicy(options, RolePolicy.Organizer, nameof(UserRole.Admin),
				nameof(UserRole.Organizer));
		}

		private static void AddPolicy(AuthorizationOptions options, string policyName,
			params string[] requiredValues)
		{
			options.AddPolicy(policyName,
				policy => policy.RequireClaim(RolePolicy.UserRoleClaim, requiredValues));
		}
	}
}