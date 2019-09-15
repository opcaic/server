namespace OPCAIC.Application.Dtos.Games
{
	public class GameDetailDto : GamePreviewDto
	{
		public string DefaultTournamentImage { get; set; }

		public float? DefaultTournamentImageOverlay { get; set; }

		public string DefaultTournamentThemeColor { get; set; }

		public string ConfigurationSchema { get; set; }
	}
}
