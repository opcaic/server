using System;
using System.Collections.Generic;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Broker
{
	public class WorkerInfo
	{
		public string Identity { get; set; }
		public Guid? CurrentJob { get; set; }
		public IEnumerable<string> Games { get; set; }
		public WorkerStats Stats { get; set; }
	}
}