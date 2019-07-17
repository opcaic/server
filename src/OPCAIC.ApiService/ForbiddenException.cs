using Microsoft.AspNetCore.Http;

namespace OPCAIC.ApiService
{
	public class ForbiddenException: ApiException
	{
		public ForbiddenException(string message)
			:base(StatusCodes.Status403Forbidden, message) { }
	}
}
