using System;

namespace OPCAIC.Application.Exceptions
{
	[Serializable]
	public class NotFoundException : Exception
	{
		public string Resource { get; }
		public long? ResourceId { get; }

		public NotFoundException(string resourceName, string message) :
				base(message)
		{
			Resource = resourceName;
		}

		public NotFoundException(string resourceName) :
				this(resourceName, $"Specified '{resourceName}' was not found.")
		{
		}

		public NotFoundException(string resourceName, long resourceId) :
				this(resourceName, $"Resource '{resourceName}' with id {resourceId} was not found.")
		{
			ResourceId = resourceId;
		}
	}
}