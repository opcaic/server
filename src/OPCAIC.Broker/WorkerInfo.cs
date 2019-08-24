using System;

namespace OPCAIC.Broker
{
	public class WorkerInfo
	{
		public string Identity { get; set; }
		public Guid? CurrentJob { get; set; }
	}
}