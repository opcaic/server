using System;
using MediatR;

namespace OPCAIC.Application.Infrastructure.Events
{
	public abstract class GameEvent : INotification
	{
		public long GameId { get; set; }
	}

	public abstract class TournamentEvent : GameEvent
	{
		public long TournamentId { get; set; }
	}

	public abstract class SubmissionEvent : TournamentEvent
	{
		public long SubmissionId { get; set; }
	}

	public abstract class SubmissionValidationEvent : SubmissionEvent
	{
		public Guid JobId { get; set; }

		public long ValidationId { get; set; }
	}

	public abstract class MatchEvent : TournamentEvent
	{
		public long MatchId { get; set; }
	}

	public abstract class MatchExecutionEvent : MatchEvent
	{
		public Guid JobId { get; set; }

		public long ExecutionId { get; set; }
	}
}