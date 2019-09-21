using OPCAIC.Application.Tournaments.Models;

namespace OPCAIC.ApiService.Models.Tournaments
{
	public class TournamentDetailModel : TournamentPreviewDto
	{
		public string Description { get; set; }

		public string MenuData { get; set; }
	}
}