using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     A tournament between AI bots.
	/// </summary>
	public class Tournament : SoftDeletableEntity
	{
		/// <summary>
		///     Name of the tournament.
		/// </summary>
		[Required]
		[MaxLength(StringLengths.TournamentName)]
		public string Name { get; set; }

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
	}
}