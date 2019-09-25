using Newtonsoft.Json.Linq;
using OPCAIC.Application.Dtos.Games;
using OPCAIC.Application.Games.Models;
using OPCAIC.Application.Games.Queries;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Enums;

namespace OPCAIC.ApiService.Models.Games
{
	public class GameDetailModel : GamePreviewModel, IMapFrom<GameDetailDto>
	{
		public string Key { get; set; }

		public GameType Type { get; set; }

		public JObject ConfigurationSchema { get; set; }

		public string DefaultTournamentImageUrl { get; set; }

		public float? DefaultTournamentImageOverlay { get; set; }

		public string DefaultTournamentThemeColor { get; set; }

		public string Description { get; set; }

		public long MaxAdditionalFilesSize { get; set; }
	}
}