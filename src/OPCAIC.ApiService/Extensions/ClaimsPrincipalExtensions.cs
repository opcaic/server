using System;
using System.Security.Claims;
using OPCAIC.ApiService.Security;
using OPCAIC.Domain.Enums;

namespace OPCAIC.ApiService.Extensions
{
	public static class ClaimsPrincipalExtensions
	{
		public static UserRole GetUserRole(this ClaimsPrincipal user)
		{
			return Enum.TryParse<UserRole>(user.FindFirstValue(RolePolicy.UserRoleClaim), out var role) ?
				role
				: UserRole.None;
		}

		public static long GetId(this ClaimsPrincipal user)
		{
			return long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
		}

		public static long TryGetId(this ClaimsPrincipal user)
		{
			return long.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
				? id
				: long.MinValue;
		}

		public static bool TryGetId(this ClaimsPrincipal user, out long id)
		{
			return long.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out id);
		}
	}
}