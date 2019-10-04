using Newtonsoft.Json.Linq;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Games.Models
{
	public class GameDetailDto : GamePreviewDto, IMapFrom<Game>
	{
		public JObject ConfigurationSchema { get; set; }

		public string DefaultTournamentImageUrl { get; set; }

		public float? DefaultTournamentImageOverlay { get; set; }

		public string DefaultTournamentThemeColor { get; set; }

		public string Description { get; set; }

		public long MaxAdditionalFilesSize { get; set; }
	}
}