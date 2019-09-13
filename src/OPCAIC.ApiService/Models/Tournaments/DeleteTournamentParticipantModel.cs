using Microsoft.AspNetCore.Mvc;

namespace OPCAIC.ApiService.Models.Tournaments
{
	public class DeleteTournamentParticipantModel
	{
		[FromRoute(Name = "email")]
		public string Email { get; set; }
	}
}