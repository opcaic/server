using System;

namespace OPCAIC.Application.Dtos.Broker
{
	public class WorkItemDto
	{
		public DateTime QueuedTime { get; set; }
		public WorkMessageBaseDto Payload { get; set; }
	}
}