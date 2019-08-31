using System;

namespace OPCAIC.ApiService.Models.Broker
{
	public class WorkMessageBaseModel
	{
		public Guid JobId { get; set; }
		public string Game { get; set; }
	}
}