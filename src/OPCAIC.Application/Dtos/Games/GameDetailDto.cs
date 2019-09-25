using OPCAIC.Application.Games.Models;
using OPCAIC.Application.Games.Queries;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Games
{
	public class GameDetailDto : GamePreviewModel, IMapFrom<Game>
	{
		public string Key { get; set; }

		public GameType Type { get; set; }

		public string DefaultTournamentImageUrl { get; set; }

		public float? DefaultTournamentImageOverlay { get; set; }

		public string DefaultTournamentThemeColor { get; set; }

		public string ConfigurationSchema { get; set; }

		public string Description { get; set; }

		public long MaxAdditionalFilesSize { get; set; }
	}
}
