using Microsoft.AspNetCore.Authorization;

namespace OPCAIC.ApiService.Security
{
	public static class AuthorizationConfiguration
	{
		public static void Setup(AuthorizationOptions options)
		{
			AddPolicy(options, RolePolicy.Admin, nameof(UserRole.Admin));
			AddPolicy(options, RolePolicy.Organizer, nameof(UserRole.Admin),
				nameof(UserRole.Organizer));
			AddPolicy(options, RolePolicy.User, nameof(UserRole.Admin), nameof(UserRole.Organizer),
				nameof(UserRole.User));

			options.AddPolicy(RolePolicy.Public, policy => policy.RequireAuthenticatedUser());
		}

		private static void AddPolicy(AuthorizationOptions options, string policyName,
			params string[] requiredValues) => options.AddPolicy(policyName,
			policy => policy.RequireClaim(RolePolicy.PolicyName, requiredValues));
	}
}