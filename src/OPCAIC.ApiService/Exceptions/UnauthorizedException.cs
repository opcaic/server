using Microsoft.AspNetCore.Http;

namespace OPCAIC.ApiService.Exceptions
{
	public class UnauthorizedException : ApiException
	{
		public UnauthorizedException(string message)
			: base(StatusCodes.Status401Unauthorized, message)
		{
		}
	}
}