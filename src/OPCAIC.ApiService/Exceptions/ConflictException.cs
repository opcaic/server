using Microsoft.AspNetCore.Http;

namespace OPCAIC.ApiService.Exceptions
{
	public class ConflictException : ApiException
	{
		public ConflictException(string message)
			: base(StatusCodes.Status409Conflict, message)
		{
		}
	}
}