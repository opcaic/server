using System;
using System.Collections.Generic;
using OPCAIC.Utils;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///   Base class for all database entities.
	/// </summary>
	public abstract class Entity
	{
		/// <summary>
		///   Primary key of this entity
		/// </summary>
		public long Id { get; set; }

		/// <summary>
		///   Timestamp when this entity was created.
		/// </summary>
		public DateTime Created { get; set; }

		/// <summary>
		///   Timestamp of last update of this entity.
		/// </summary>
		public DateTime Updated { get; set; }
	}

	public abstract class SoftDeletableEntity : Entity, ISoftDeletable
	{
		/// <inheritdoc />
		public bool IsDeleted { get; set; }
	}

	public interface ISoftDeletable
	{
		/// <summary>
		///   Flag whether the entity should be considered deleted (so we never actually delete any data).
		/// </summary>
		bool IsDeleted { get; set; }
	}

	public class Submission : SoftDeletableEntity
	{
		public string Path { get; set; }
		public string Author { get; set; }
		public bool IsActive { get; set; }
	}

	public class MatchExecution : SoftDeletableEntity
	{
		public long MatchId { get; set; }
		public Match Match { get; set; }

		/// <summary>
		///   Timestamp when this match was executed.
		/// </summary>
		public DateTime Executed { get; set; }

		/// <summary>
		///   Results of individual bots in this match.
		/// </summary>
		public virtual IList<SubmissionMatchResult> BotResults { get; set; }
	}

	public class Match : SoftDeletableEntity
	{
		public long TournamentId { get; set; }
		public Tournament Tournament { get; set; }
		public virtual IList<MatchExecution> Executions { get; set; }
		public virtual IList<Submission> Participants { get; set; }
		public MatchExecution LastExecution => Executions.ArgMaxOrDefault(e => e.Executed);
	}

	public class SubmissionMatchResult : SoftDeletableEntity
	{
		public long SubmissionId { get; set; }
		public Submission Submission { get; set; }
		public double Score { get; set; }
		public string AdditionalData { get; set; }
	}
}
