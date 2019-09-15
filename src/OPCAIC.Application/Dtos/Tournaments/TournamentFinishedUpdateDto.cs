using System;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Infrastructure.Dtos.Tournaments
{
	public class TournamentFinishedUpdateDto
	{
		public TournamentState State { get; set; }

		public DateTime? EvaluationFinished { get; set; }
	}
}