using Newtonsoft.Json.Linq;

namespace OPCAIC.ApiService.Models.Games
{
	public class NewGameModel
	{
		public string Name { get; set; }

		public string Key { get; set; }

		public JObject ConfigurationSchema { get; set; }

		public long MaxAdditionalFilesSize { get; set; }
	}
}