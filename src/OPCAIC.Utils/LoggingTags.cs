﻿namespace OPCAIC.Utils
{
	public static class LoggingTags
	{
		public const string HttpStatusCode = "StatusCode";
		public const string HttpRequestPath = "RequestPath";
		public const string HttpRequestMethod = "HttpRequestMethod";
		public const string HttpElapsedTime = "ElapsedMilliseconds";

		public const string RefreshToken = "RefreshToken";

		public const string IdentityResult = "IdentityResult";

		public const string UserId = "UserId";
		public const string UserEmail = "UserEmail";
		public const string UserName = "Username";
		public const string UserRole = "UserRole";

		public const string MailId = "MailId";

		public const string MachineName = "machine_name";
		public const string AppVersion = "app_version";

		public const string JobId = "JobId";
		public const string JobPayload = "JobPayload";
		public const string JobType = "JobType";

		public const string SubmissionId = "SubmissionId";
		public const string ExecutionId = "ExecutionId";
		public const string ValidationId = "ValidationId";
		public const string TournamentId = "TournamentId";

		public const string Game = "Game";
		public const string GameModuleEntryPoint = "EntryPoint";
		public const string ConnectorIdentity = "Identity";

		public const string GameModuleProcessExitCode = "GameModuleProcessExitCode";
		public const string GameModuleProcessId = "GameModulePID";
	}
}