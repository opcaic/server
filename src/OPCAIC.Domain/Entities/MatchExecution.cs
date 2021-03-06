﻿using System;
using System.Collections.Generic;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Domain.Entities
{
	/// <summary>
	///     A single execution of a match in a tournament.
	/// </summary>
	public class MatchExecution : Entity, IWorkerJob
	{
		/// <summary>
		///     Unique Id of the match.
		/// </summary>
		public long MatchId { get; set; }

		/// <summary>
		///     Math which was executed.
		/// </summary>
		public virtual Match Match { get; set; }

		/// <summary>
		///     Result of the executor game module entry point.
		/// </summary>
		public EntryPointResult ExecutorResult { get; set; }

		/// <summary>
		///     Results of individual bots in this match.
		/// </summary>
		public virtual IList<SubmissionMatchResult> BotResults { get; set; }

		/// <summary>
		///     Additional key-value data for this match provided by the game module. Serialized as a JSON object.
		/// </summary>
		public string AdditionalData { get; set; }

		/// <inheritdoc />
		public Guid JobId { get; set; }

		/// <inheritdoc />
		public WorkerJobState State { get; set; }

		/// <inheritdoc />
		public DateTime? Executed { get; set; }

		/// <inheritdoc />
		public string Exception { get; set; }
	}
}