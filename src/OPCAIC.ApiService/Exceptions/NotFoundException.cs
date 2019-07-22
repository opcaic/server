using Microsoft.AspNetCore.Http;

namespace OPCAIC.ApiService.Exceptions
{
	public class NotFoundException : ApiException
	{
		public NotFoundException(string resourceName, int resourceId) :
			base(StatusCodes.Status404NotFound,
				$"Resource '{resourceName}' with id {resourceId} was not found.")
		{
		}
	}
}