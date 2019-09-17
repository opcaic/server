using OPCAIC.Domain.Enums;

namespace OPCAIC.ApiService.Models.Games
{
	public class NewGameModel
	{
		public string Name { get; set; }

		public string Key { get; set; }

		public string ImageUrl { get; set; }

		public string DefaultTournamentImageUrl { get; set; }

		public float? DefaultTournamentImageOverlay { get; set; }

		public string DefaultTournamentThemeColor { get; set; }

		public string Description { get; set; }

		public GameType Type { get; set; }

		public long MaxAdditionalFilesSize { get; set; }
	}
}