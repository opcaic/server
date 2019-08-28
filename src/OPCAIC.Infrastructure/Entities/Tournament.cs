using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     A tournament between AI bots.
	/// </summary>
	public class Tournament : SoftDeletableEntity
	{
		/// <summary>
		///     Id of the owner of the tournament.
		/// </summary>
		public long OwnerId { get; set; }
		
		/// <summary>
		///     Owner of the tournament.
		/// </summary>
		public virtual User Owner { get; set; }

		/// <summary>
		///     List of managers for this tournaments.
		/// </summary>
		public virtual IList<TournamentManager> Managers { get; set; }

		/// <summary>
		///     Name of the tournament.
		/// </summary>
		[Required]
		[MaxLength(StringLengths.TournamentName)]
		public string Name { get; set; }

		/// <summary>
		///     JSON serialized configuration of the game for this tournament.
		/// </summary>
		public string ConfigurationJson { get; set; }

		/// <summary>
		///     Description of the tournament.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		///     Rules of the tournament.
		/// </summary>
		public string Rules { get; set; }

		/// <summary>
		///     Id of the game this tournament is in.
		/// </summary>
		public long GameId { get; set; }

		/// <summary>
		///     Game the tournament is in.
		/// </summary>
		public virtual Game Game { get; set; }

		/// <summary>
		///     All submissions submitted to this tournament.
		/// </summary>
		public virtual IList<Submission> Submissions { get; set; }

		/// <summary>
		///     The format of this tournament.
		/// </summary>
		public TournamentFormat Format { get; set; }

		/// <summary>
		///     The scope of the tournament.
		/// </summary>
		public TournamentScope Scope { get; set; }

		/// <summary>
		///     Ranking strategy in this tournament.
		/// </summary>
		public TournamentRankingStrategy RankingStrategy { get; set; }

		/// <summary>
		///     Availability of this tournament.
		/// </summary>
		public TournamentAvailability Availability { get; set; }

		/// <summary>
		///     State of the tournament.
		/// </summary>
		public TournamentState State { get; set; }

		/// <summary>
		///     When the tournament was published on the website.
		/// </summary>
		public DateTime Published { get; set; }

		/// <summary>
		///     When the tournament's evaluation started (or will start in case of deadline tournaments).
		/// </summary>
		public DateTime EvaluationStarted { get; set; }

		/// <summary>
		///     When the evaluation finished.
		/// </summary>
		public DateTime EvaluationFinished { get; set; }

		/// <summary>
		///		Participants of tournament.
		/// </summary>
		public virtual ICollection<TournamentParticipant> Participants { get; set; }
	}

	public class TournamentManager
	{
		public long UserId { get; set; }

		public virtual User User { get; set; }
		public long TournamentId { get; set; }

		public virtual Tournament Tournament { get; set; }

		internal static void OnModelCreating(EntityTypeBuilder<TournamentManager> builder)
		{
			// mapping table, make sure the same mapping does not exist twice
			builder.HasKey(nameof(UserId), nameof(TournamentId));
		}
	}
}