using OPCAIC.Application.Infrastructure.Validation;

namespace OPCAIC.Application.Exceptions
{
	public class UserIsNotManagerOfTournamentException : BusinessException
	{
		/// <inheritdoc />
		public UserIsNotManagerOfTournamentException(long tournamentId, string userEmail)
			: base(new Error(tournamentId, userEmail))
		{
		}

		private Error MyError => (Error)base.Error;
		public long TournamentId => MyError.TournamentId;
		public string UserEmail => MyError.UserEmail;

		private new class Error : ApplicationError
		{
			/// <inheritdoc />
			public Error(long tournamentId, string userEmail) : base(ValidationErrorCodes.UserIsNotManagerOfTournament,
				$"User with email {userEmail} is not a manager of tournament with id {tournamentId}")
			{
				TournamentId = tournamentId;
				UserEmail = userEmail;
			}

			public long TournamentId { get; }
			public string UserEmail { get; }
		}
	}
}
