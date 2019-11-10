using System;
using System.Runtime.Serialization;

namespace OPCAIC.Application.Exceptions
{
	[Serializable]
	public class AppException : Exception
	{
		public AppException() { }
		public AppException(string message) : base(message) { }
		public AppException(string message, Exception inner) : base(message, inner) { }

		protected AppException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}

	[Serializable]
	public class NotFoundException : AppException
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