namespace OPCAIC.Worker.Sandboxing
{
	public class SandboxSettings
	{
		public int MaxMemory { get; set; }
		public int MaxParallelism { get; set; }
		public bool ShareNetwork { get; set; }
	}
}
