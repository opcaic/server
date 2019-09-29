using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace OPCAIC.Common
{
	public static class LoggingEvents
	{
		private static EventId CreateEvent(int id, [CallerMemberName] string name = null)
		{
			return new EventId(id, name);
		}

		public static readonly EventId LoginSuccess = CreateEvent(100);
		public static readonly EventId LoginInvalidPassword = CreateEvent(101);
		public static readonly EventId LoginInvalidEmail = CreateEvent(102);
		public static readonly EventId LoginNotAllowed = CreateEvent(103);
		public static readonly EventId LoginLockout = CreateEvent(104);

		public static readonly EventId UserCreate = CreateEvent(200);

		public static readonly EventId UserForgotPassword = CreateEvent(300);
		public static readonly EventId UserPasswordReset = CreateEvent(301);
		public static readonly EventId UserPasswordChange = CreateEvent(302);
		public static readonly EventId UserEmailConfirm = CreateEvent(303);

		public static readonly EventId SubmissionCreate = CreateEvent(400);
		public static readonly EventId SubmissionUpdate = CreateEvent(401);
		public static readonly EventId SubmissionValidationQueued = CreateEvent(402);
		public static readonly EventId SubmissionValidationUpdated = CreateEvent(403);
		public static readonly EventId SubmissionValidationUploadResults = CreateEvent(404);

		public static readonly EventId MatchExecutionQueued = CreateEvent(500);
		public static readonly EventId MatchExecutionUpdated = CreateEvent(501);
		public static readonly EventId MatchExecutionUploadResults = CreateEvent(503);

		public static readonly EventId TournamentCreated = CreateEvent(600);
		public static readonly EventId TournamentUpdated = CreateEvent(601);
		public static readonly EventId TournamentStateChanged = CreateEvent(602);
		public static readonly EventId TournamentMatchesGeneration= 603;

		public static readonly EventId GameCreated = CreateEvent(700);
		public static readonly EventId GameUpdated = CreateEvent(701);

		public static readonly EventId DocumentCreated = CreateEvent(801);
		public static readonly EventId DocumentUpdated = CreateEvent(802);

		public static readonly EventId JwtRefreshTokenValidationFailed = CreateEvent(9000);

		public static readonly EventId Startup = CreateEvent(1000);
		public static readonly EventId WorkerConnection = CreateEvent(2000);

		public static readonly EventId JobExecutionFailure = CreateEvent(4000);
		public static readonly EventId GameModuleFailure = CreateEvent(4001);
		public static readonly EventId JobAborted = CreateEvent(3001);

		public static readonly EventId MailSentSuccess = CreateEvent(8000);
		public static readonly EventId MailSentFailed = CreateEvent(8001);

		public static readonly EventId RequestTooLong = CreateEvent(9000);
	}
}