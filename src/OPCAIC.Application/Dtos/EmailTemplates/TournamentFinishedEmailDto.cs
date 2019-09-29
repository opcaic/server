namespace OPCAIC.Application.Dtos.EmailTemplates
{
	public class TournamentFinishedEmailDto : EmailDtoBase
	{
		/// <inheritdoc />
		public TournamentFinishedEmailDto(string tournamentUrl, string tournamentName)
		{
			TournamentUrl = tournamentUrl;
			TournamentName = tournamentName;
		}

		/// <inheritdoc />
		public override string TemplateName => "tournamentFinishedEmail";

		public string TournamentUrl { get; }

		public string TournamentName { get; }
	}
}