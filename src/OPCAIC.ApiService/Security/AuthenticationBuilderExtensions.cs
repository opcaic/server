using Microsoft.AspNetCore.Authorization;

namespace OPCAIC.ApiService.Security
{
	public static class AuthenticationConfiguration
	{
		public static void Setup(AuthorizationOptions options)
		{
			AddPolicy(options, RolePolicy._Admin, nameof(UserRole.Admin));
			AddPolicy(options, RolePolicy._Organizer, nameof(UserRole.Admin), nameof(UserRole.Organizer));
			AddPolicy(options, RolePolicy._User, nameof(UserRole.Admin), nameof(UserRole.Organizer), nameof(UserRole.User));

			options.AddPolicy(RolePolicy._Public, policy => policy.RequireAuthenticatedUser());
		}

		private static void AddPolicy(AuthorizationOptions options, string policyName, params string[] requiredValues)
		{
			options.AddPolicy(policyName, policy => policy.RequireClaim(RolePolicy._PolicyName, requiredValues));
		}
	}
}
