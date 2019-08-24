using System;
using System.Collections.Generic;
using System.Linq;
using OPCAIC.Infrastructure.Dtos.Broker;

namespace OPCAIC.Broker
{
	public class WorkerInfo
	{
		public string Identity { get; set; }
		public Guid? CurrentJob { get; set; }
	}

	public class BrokerStats
	{
		public List<WorkerInfo> Workers { get; set; }
	}
}