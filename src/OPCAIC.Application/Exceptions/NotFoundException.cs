using System;

namespace OPCAIC.Application.Exceptions
{
	[Serializable]
	public class NotFoundException : Exception
	{
		public string Resource { get; }
		public long? ResourceId { get; }

		public NotFoundException(string resourceName) :
				base($"Specified '{resourceName}' was not found.")
		{
			Resource = resourceName;
			ResourceId = null;
		}

		public NotFoundException(string resourceName, long resourceId) :
				base($"Resource '{resourceName}' with id {resourceId} was not found.")
		{
			Resource = resourceName;
			ResourceId = resourceId;
		}
	}
}