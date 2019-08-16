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
		///     Name of the game. Should be unique on the platform.
		/// </summary>
		[Required]
		[MaxLength(StringLengths.GameName)]
		public string Name { get; set; }


		/// <summary>
		///     Description of the game.
		/// </summary>
		public string Description { get; set; }

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