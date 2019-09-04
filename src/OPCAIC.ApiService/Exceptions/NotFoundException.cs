using Microsoft.AspNetCore.Http;

namespace OPCAIC.ApiService.Exceptions
{
	public class NotFoundException : ApiException
	{
		public string Resource { get; }
		public long ResourceId { get; }

		public NotFoundException(string resourceName, long resourceId) :
			base(StatusCodes.Status404NotFound,
				$"Resource '{resourceName}' with id {resourceId} was not found.", null)
		{
			Resource = resourceName;
			ResourceId = resourceId;
		}
	}
}