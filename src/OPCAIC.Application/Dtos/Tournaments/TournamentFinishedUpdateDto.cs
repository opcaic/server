using System;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Tournaments
{
	public class TournamentFinishedUpdateDto
	{
		public TournamentState State { get; set; }

		public DateTime? EvaluationFinished { get; set; }
	}
}