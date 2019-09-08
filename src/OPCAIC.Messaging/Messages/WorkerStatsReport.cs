namespace OPCAIC.Messaging.Messages
{
	public class WorkerStatsReport
	{
		public long AllocatedBytes { get; set; }
		public long Gen0Collections { get; set; }
		public long Gen1Collections { get; set; }
		public long Gen2Collections { get; set; }
		public long DiskSpace { get; set; }
		public long DiskSpaceLeft { get; set; }
	}
}