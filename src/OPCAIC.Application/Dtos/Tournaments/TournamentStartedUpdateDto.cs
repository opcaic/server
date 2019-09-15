using System;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Tournaments
{
	public class TournamentStartedUpdateDto
	{
		public TournamentState State { get; set; }

		public DateTime? EvaluationStarted { get; set; }
	}
}