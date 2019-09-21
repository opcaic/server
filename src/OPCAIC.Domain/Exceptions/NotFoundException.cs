﻿using System;

namespace OPCAIC.Domain.Exceptions
{
	[Serializable]
	public class NotFoundException : Exception
	{
		public string Resource { get; }
		public long ResourceId { get; }

		public NotFoundException(string resourceName, long resourceId) :
				base($"Resource '{resourceName}' with id {resourceId} was not found.")
		{
			Resource = resourceName;
			ResourceId = resourceId;
		}
	}
}