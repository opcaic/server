namespace OPCAIC.ApiService.Models.Tournaments
{
	public class NewTournamentInvitationsModel
	{			
		public long TournamentId { get; set; }
		public string[] Emails { get; set; }
	}
}
