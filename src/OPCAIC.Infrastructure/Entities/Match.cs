using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using OPCAIC.Infrastructure.Enums;

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
		///     List of execution attempts for this match.
		/// </summary>
		public virtual IList<MatchExecution> Executions { get; set; }

		/// <summary>
		///     Last execution of this match.
		/// </summary>
		[NotMapped]
		public MatchExecution LastExecution
			=> Executions?.AsQueryable().OrderBy(e => e.Created).FirstOrDefault();

		/// <summary>
		///     Simplified state of this match.
		/// </summary>
		[NotMapped]
		public MatchState State
		{
			get
			{
				switch (LastExecution.ExecutorResult)
				{
					case GameModuleEntryPointResult.NotExecuted:
						return MatchState.Queued;

					case GameModuleEntryPointResult.Success:
						return MatchState.Executed;

					case GameModuleEntryPointResult.UserError:
					case GameModuleEntryPointResult.ModuleError:
					case GameModuleEntryPointResult.PlatformError:
						return MatchState.Failed;

					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}