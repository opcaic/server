using Microsoft.AspNetCore.Http;

namespace OPCAIC.ApiService.Exceptions
{
	public class ForbiddenException : ApiException
	{
		public ForbiddenException(string message)
			: base(StatusCodes.Status403Forbidden, message)
		{
		}
	}
}