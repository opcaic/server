using System;
using OPCAIC.Application.Dtos.Games;
using OPCAIC.Application.Infrastructure.AutoMapper;

namespace OPCAIC.Application.Games.Models
{
	public class GamePreviewModel : IMapFrom<GameDetailDto>
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public DateTime Created { get; set; }

		public int ActiveTournamentsCount { get; set; }

		public string ImageUrl { get; set; }
	}
}