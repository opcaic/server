namespace OPCAIC.Application.Dtos.EmailTemplates
{
	public class TournamentFinishedEmailDto : EmailDtoBase
	{
		/// <inheritdoc />
		public override string TemplateName => "tournamentFinishedEmail";

		public string TournamentUrl { get; set; }

		public string TournamentName { get; set; }
	}
}