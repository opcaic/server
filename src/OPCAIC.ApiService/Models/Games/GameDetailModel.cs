using Newtonsoft.Json.Linq;

namespace OPCAIC.ApiService.Models.Games
{
	public class GameDetailModel : GamePreviewModel
	{
		public JObject ConfigurationSchema { get; set; }

		public string DefaultTournamentImage { get; set; }

		public float? DefaultTournamentImageOverlay { get; set; }

		public string DefaultTournamentThemeColor { get; set; }
	}
}