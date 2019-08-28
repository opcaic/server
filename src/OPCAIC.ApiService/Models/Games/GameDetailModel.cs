using Newtonsoft.Json.Linq;

namespace OPCAIC.ApiService.Models.Games
{
	public class GameDetailModel : GamePreviewModel
	{
		public JObject ConfigurationSchema { get; set; }
	}
}