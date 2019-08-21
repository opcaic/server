using System.Security.Claims;

namespace OPCAIC.ApiService.Extensions
{
	public static class ClaimsPrincipalExtensions
	{
		public static long GetId(this ClaimsPrincipal user)
		{
			return long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier));
		}
	}
}