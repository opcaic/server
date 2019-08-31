using System;

namespace OPCAIC.Infrastructure.Dtos.Games
{
	public class GamePreviewDto
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public DateTime Created { get; set; }

		public int ActiveTournamentsCount { get; set; }

		public string ImageUrl { get; set; }
	}
}