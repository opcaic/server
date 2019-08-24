using System;

namespace OPCAIC.ApiService.Models.Broker
{
	public class WorkItemFilterModel
	{
		public DateTime? Since { get; set; }

		public DateTime? Until { get; set; }

		public string Game { get; set; }
	}
}