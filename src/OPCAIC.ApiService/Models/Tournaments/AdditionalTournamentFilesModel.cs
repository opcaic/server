using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OPCAIC.ApiService.Models.Tournaments
{
	public class AdditionalTournamentFilesModel
	{
		[FromRoute(Name = "id")]
		public long TournamentId { get; set; }

		public IFormFile Archive { get; set; }
	}
}