namespace OPCAIC.Infrastructure.Dtos.Tournaments
{
	public class TournamentDetailDto : TournamentPreviewDto
	{
		public string Description { get; set; }

		public string Configuration { get; set; }
	}
}