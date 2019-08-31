using System;
using System.Security.Claims;
using OPCAIC.ApiService.Security;

namespace OPCAIC.ApiService.Extensions
{
	public static class ClaimsPrincipalExtensions
	{
		public static UserRole GetUserRole(this ClaimsPrincipal user)
		{
			return Enum.Parse<UserRole>(user.FindFirstValue(RolePolicy.UserRoleClaim));
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
	}
}