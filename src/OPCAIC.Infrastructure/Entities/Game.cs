using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     Basic information about a game in the platform.
	/// </summary>
	public class Game : SoftDeletableEntity
	{
		/// <summary>
		///    Name of the game. Should be unique on the platform.
		/// </summary>
		[Required]
		[MaxLength(StringLengths.GameName)]
		public string Name { get; set; }

		/// <summary>
		///     All tournaments in this game.
		/// </summary>
		public virtual IList<Tournament> Tournaments { get; set; }

		internal static void OnModelCreating(EntityTypeBuilder<Game> builder)
		{
			// unique constraints cannot be performed by DataAnnotations yet
			builder.HasIndex(g => g.Name).IsUnique();
		}
	}
}