namespace OPCAIC.Application.Dtos.EmailTemplates
{
	public class TournamentInvitationEmailDto : EmailDtoBase
	{
		public override string TemplateName => "tournamentInvitationEmail";

		public string TournamentUrl { get; set; }

		public string TournamentName { get; set; }
	}
}