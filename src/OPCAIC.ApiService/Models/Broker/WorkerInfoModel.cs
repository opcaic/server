using System;

namespace OPCAIC.ApiService.Models.Broker
{
	public class WorkerInfoModel
	{
		public string Identity { get; set; }

		public Guid? CurrentJob { get; set; }

		public string[] Games { get; set; }
	}
}