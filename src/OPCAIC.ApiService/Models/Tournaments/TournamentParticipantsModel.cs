namespace OPCAIC.ApiService.Models.Tournaments
{
	public class NewTournamentParticipantsModel
	{			
		public long TournamentId { get; set; }
		public string[] Emails { get; set; }
	}
}
