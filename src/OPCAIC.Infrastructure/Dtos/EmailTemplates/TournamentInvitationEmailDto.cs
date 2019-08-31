namespace OPCAIC.Infrastructure.Dtos.EmailTemplates
{
	public class TournamentInvitationEmailDto : EmailDtoBase
	{
		public override string TemplateName => "tournamentInvitationEmail";

		public string TournamentUrl { get; set; }
	}
}