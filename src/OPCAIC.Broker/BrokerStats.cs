using System.Collections.Generic;

namespace OPCAIC.Broker
{
	public class BrokerStats
	{
		public List<WorkerInfo> Workers { get; set; }

		public int JobCount { get; set; }

		public ICollection<string> Games { get; set; }
	}
}