using System;

namespace OPCAIC.ApiService.Models.Games
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