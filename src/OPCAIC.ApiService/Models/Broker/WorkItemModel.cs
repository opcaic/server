using System;

namespace OPCAIC.ApiService.Models.Broker
{
	public class WorkItemModel
	{
		public DateTime QueuedTime { get; set; }
		public WorkMessageBaseModel Payload { get; set; }
	}
}