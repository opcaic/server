using System;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Dtos.Tournaments
{
	public class TournamentStartedUpdateDto
	{
		public TournamentState State { get; set; }

		public DateTime? EvaluationStarted { get; set; }
	}
}