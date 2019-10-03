using OPCAIC.Application.Infrastructure.Validation;

namespace OPCAIC.Application.Exceptions
{
	public class UserIsAlreadyManagerOfTournamentException : BusinessException
	{
		/// <inheritdoc />
		public UserIsAlreadyManagerOfTournamentException(long tournamentId, string userEmail)
			: base(new Error(tournamentId, userEmail))
		{
		}

		private Error MyError => (Error)base.Error;
		public long TournamentId => MyError.TournamentId;
		public string UserEmail => MyError.UserEmail;

		private new class Error : ApplicationError
		{
			/// <inheritdoc />
			public Error(long tournamentId, string userEmail) : base(ValidationErrorCodes.UserIsAlreadyManagerOfTournament,
				$"User with email {userEmail} is already a manager of tournament with id {tournamentId}")
			{
				TournamentId = tournamentId;
				UserEmail = userEmail;
			}

			public long TournamentId { get; }
			public string UserEmail { get; }
		}
	}
}