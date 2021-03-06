﻿namespace OPCAIC.Domain.Enums
{
	/// <summary>
	///     Simplified match state for frontend.
	/// </summary>
	public enum MatchState
	{
		/// <summary>
		///     Match is queued for execution. The match may already have some existing executions.
		/// </summary>
		Queued,

		/// <summary>
		///     Match has been successfully executed.
		/// </summary>
		Executed,

		/// <summary>
		///     Last match execution failed.
		/// </summary>
		Failed,

		/// <summary>
		///     Last match execution was cancelled.
		/// </summary>
		Cancelled
	}
}