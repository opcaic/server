﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     Represents a single match in a tournament.
	/// </summary>
	public class Match : SoftDeletableEntity
	{
		/// <summary>
		///     Index of the match in a tournament.
		/// </summary>
		public long Index { get; set; }

		/// <summary>
		///     Id of the tournament to which this match belongs.
		/// </summary>
		public long TournamentId { get; set; }

		/// <summary>
		///     Tournament this match belongs to.
		/// </summary>
		[Required]
		public virtual Tournament Tournament { get; set; }

		/// <summary>
		///     Reference to mapping table of matches and their participants.
		/// </summary>
		public virtual IList<SubmissionParticipation> Participations { get; set; }

		/// <summary>
		///     List of participating submissions.
		/// </summary>
		[NotMapped]
		public IEnumerable<Submission> Submissions => Participations.Select(p => p.Submission);

		/// <summary>
		///     Authors of the participating submissions.
		/// </summary>
		public virtual IList<User> Participators { get; set; }

		/// <summary>
		///     List of execution attempts for this match.
		/// </summary>
		public virtual IList<MatchExecution> Executions { get; set; }

		/// <summary>
		///     Last execution of this match.
		/// </summary>
		public virtual MatchExecution LastExecution
			=> Executions?.AsQueryable().OrderBy(e => e.Created).FirstOrDefault();

		/// <summary>
		///     Results of this match.
		/// </summary>
		public virtual IList<SubmissionMatchResult> Results
			=> LastExecution?.BotResults;
	}
}