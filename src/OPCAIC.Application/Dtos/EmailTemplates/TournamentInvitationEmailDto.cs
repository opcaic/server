namespace OPCAIC.Application.Dtos.EmailTemplates
{
	public class TournamentInvitationEmailDto : EmailDtoBase
	{
		/// <inheritdoc />
		public TournamentInvitationEmailDto(string tournamentUrl, string tournamentName)
		{
			TournamentUrl = tournamentUrl;
			TournamentName = tournamentName;
		}

		public override string TemplateName => "tournamentInvitationEmail";

		public string TournamentUrl { get; }

		public string TournamentName { get; }
	}
}