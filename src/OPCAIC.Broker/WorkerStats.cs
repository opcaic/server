using System;

namespace OPCAIC.Broker
{
	public class WorkerStats
	{
		public DateTime Timestamp { get; set; }
		public long AllocatedBytes { get; set; }
		public long Gen0Collections { get; set; }
		public long Gen1Collections { get; set; }
		public long Gen2Collections { get; set; }
		public long DiskSpace { get; set; }
		public long DiskSpaceLeft { get; set; }
	}
}