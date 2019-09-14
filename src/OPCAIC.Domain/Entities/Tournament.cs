using System;
using System.Collections.Generic;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Domain.Entities
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
		public string Name { get; set; }

		/// <summary>
		///     JSON serialized configuration of the game for this tournament.
		/// </summary>
		public string Configuration { get; set; }

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
		///     All matches in this tournament.
		/// </summary>
		public virtual IList<Match> Matches { get; set; }

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
		public DateTime? Published { get; set; }

		/// <summary>
		///     When the tournament's evaluation started.
		/// </summary>
		public DateTime? EvaluationStarted { get; set; }

		/// <summary>
		///     When the evaluation finished.
		/// </summary>
		public DateTime? EvaluationFinished { get; set; }

		/// <summary>
		///     Last moment when submissions can be added to the tournament.
		/// </summary>
		public DateTime? Deadline { get; set; }

		/// <summary>
		///     Participants of tournament.
		/// </summary>
		public virtual ICollection<TournamentParticipant> Participants { get; set; }

		/// <summary>
		///     Image url that is used both in tournament list and tournament detail.
		/// </summary>
		public string ImageUrl { get; set; }

		/// <summary>
		///     Opacity of a black layer that is used over tournament's image in the detail.
		/// </summary>
		public double? ImageOverlay { get; set; }

		/// <summary>
		///     Theme color of the tournament.
		/// </summary>
		public string ThemeColor { get; set; }

		/// <summary>
		///     JSON data with information about menu items.
		/// </summary>
		public string MenuData { get; set; }

		/// <summary>
		///     How many matches to generate per day. Valid only for <see cref="TournamentScope.Ongoing" /> tournaments.
		/// </summary>
		public int? MatchesPerDay { get; set; }

		/// <summary>
		///     Maximum size in bytes on the submission archive contents.
		/// </summary>
		public long MaxSubmissionSize { get; set; }
	}
}