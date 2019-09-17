using System;

namespace OPCAIC.Application.Games.Queries
{
	public class GamePreviewModel
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public DateTime Created { get; set; }

		public int ActiveTournamentsCount { get; set; }

		public string ImageUrl { get; set; }
	}
}