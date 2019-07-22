using Microsoft.AspNetCore.Http;

namespace OPCAIC.ApiService.Exceptions
{
	public class BadRequestException : ApiException
	{
		public BadRequestException(string message)
			: base(StatusCodes.Status400BadRequest, message)
		{
		}
	}
}