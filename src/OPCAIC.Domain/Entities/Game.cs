using System.Collections.Generic;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Domain.Entities
{
	/// <summary>
	///     Basic information about a game in the platform.
	/// </summary>
	public class Game : SoftDeletableEntity
	{
		/// <summary>
		///     Human readable name of the game.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///     Key name of the game which will be used to dispatch work items to workers.
		/// </summary>
		public string Key { get; set; }

		/// <summary>
		///     Description of the game.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		///     Describes the structure of the game's configuration file.
		/// </summary>
		public string ConfigurationSchema { get; set; }

		/// <summary>
		///     All tournaments in this game.
		/// </summary>
		public virtual IList<Tournament> Tournaments { get; set; }

		/// <summary>
		///     Type of the game.
		/// </summary>
		public GameType GameType { get; set; }

		/// <summary>
		///     Image url that is used in game list.
		/// </summary>
		public string ImageUrl { get; set; }

		/// <summary>
		///     Default image URL that is used both in tournament list and tournament detail.
		/// </summary>
		public string DefaultTournamentImage { get; set; }

		/// <summary>
		///     Default opacity of a black layer that is used over tournament's image in the detail.
		/// </summary>
		public float? DefaultTournamentImageOverlay { get; set; }

		/// <summary>
		///     Default tournament theme color of the tournament.
		/// </summary>
		public string DefaultTournamentThemeColor { get; set; }

		/// <summary>
		///     Maximum size of additional files for evaluating matches in this game.
		/// </summary>
		public long MaxAdditionalFilesSize { get; set; }
	}
}