namespace OPCAIC.Application.Dtos.Tournaments
{
	public class TournamentDetailDto : TournamentPreviewDto
	{
		public string Description { get; set; }

		public string Configuration { get; set; }

		public string MenuData { get; set; }
	}
}