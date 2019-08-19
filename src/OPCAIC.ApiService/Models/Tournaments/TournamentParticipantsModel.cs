using OPCAIC.ApiService.ModelValidationHandling.Attributes;

namespace OPCAIC.ApiService.Models.Tournaments
{
	public class NewTournamentParticipants
	{			
		[ApiEmailAddresses]
		public string[] Emails { get; set; }
	}
}
