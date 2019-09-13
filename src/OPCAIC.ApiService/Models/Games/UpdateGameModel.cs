using Newtonsoft.Json.Linq;
using OPCAIC.Infrastructure;

namespace OPCAIC.ApiService.Models.Games
{
	public class UpdateGameModel
	{
		public string Name { get; set; }

		public string Key { get; set; }

		public JObject ConfigurationSchema { get; set; }

		public long MaxAdditionalFilesSize { get; set; }
	}
}