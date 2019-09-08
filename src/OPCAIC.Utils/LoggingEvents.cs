namespace OPCAIC.Utils
{
	public static class LoggingEvents
	{
		public const int LoginSuccess = 100;
		public const int LoginInvalidPassword = 101;
		public const int LoginInvalidEmail = 102;
		public const int LoginNotAllowed = 103;
		public const int LoginLockout = 104;

		public const int UserCreate = 200;

		public const int UserForgotPassword = 300;
		public const int UserPasswordReset = 301;
		public const int UserPasswordChange = 302;
		public const int UserEmailConfirm = 303;


		public const int SubmissionCreate = 400;
		public const int SubmissionUpdate = 401;
		public const int SubmissionQueueValidation = 402;

		public const int MatchQeueuExecution = 500;

		public const int TournamentCreated = 600;
		public const int TournamentStateChanged = 601;
		public const int TournamentMatchesGeneration= 602;

		public const int JwtRefreshTokenValidationFailed = 9000;

		public const int Startup = 1000;
		public const int WorkerConnection = 2000;

		public const int JobExecutionFailure = 4000;
		public const int GameModuleFailure = 4001;
		public const int JobAborted = 3001;

		public const int MailSentSuccess = 8000;
		public const int MailSentFailed = 8001;

	}
}