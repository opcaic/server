using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     Basic information about a game in the platform.
	/// </summary>
	public class Game : SoftDeletableEntity
	{
		/// <summary>
		///     Human readable name of the game.
		/// </summary>
		[Required]
		[MaxLength(StringLengths.GameName)]
		public string Name { get; set; }

		/// <summary>
		///     Key name of the game which will be used to dispatch work items to workers.
		/// </summary>
		[Required]
		[MaxLength(StringLengths.GameKey)]
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

		internal static void OnModelCreating(EntityTypeBuilder<Game> builder)
		{
			builder.HasIndex(g => g.Name).IsUnique();
		}
	}
}