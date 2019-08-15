namespace OPCAIC.Utils
{
	public static class LoggingEvents
	{
		public const int Startup = 1000;
		public const int WorkerConnection = 2000;

		public const int JobExecutionFailure = 4000;
		public const int GameModuleFailure = 4001;
		public const int JobAborted = 3001;

		public const int MailSentSuccess = 8000;
		public const int MailSentFailed = 8001;
	}
}