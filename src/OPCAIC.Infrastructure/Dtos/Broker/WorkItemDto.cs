using System;

namespace OPCAIC.Infrastructure.Dtos.Broker
{
	public class WorkItemDto
	{
		public DateTime QueuedTime { get; set; }
		public WorkMessageBaseDto Payload { get; set; }
	}
}
