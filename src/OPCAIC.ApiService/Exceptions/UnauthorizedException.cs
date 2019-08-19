using Microsoft.AspNetCore.Http;

namespace OPCAIC.ApiService.Exceptions
{
	public class UnauthorizedException : ApiException
	{
		public UnauthorizedException(string message, string code)
			: base(StatusCodes.Status401Unauthorized, message ?? nameof(UnauthorizedException),
				code)
		{
		}
	}
}