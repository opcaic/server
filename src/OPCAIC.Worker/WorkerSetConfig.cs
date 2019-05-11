namespace OPCAIC.Worker
{
	public class WorkerSetConfig
	{
		public string BrokerAddress { get; set; }
		public WorkerConfig[] Workers { get; set; }
	}
}
