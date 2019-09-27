using System;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Tournaments
{
	public class TournamentFinishedUpdateDto : TournamentStateUpdateDto
	{
		public TournamentFinishedUpdateDto(DateTime evaluationFinished)
			: base(TournamentState.Finished)
		{
			EvaluationFinished = evaluationFinished;
		}

		public DateTime EvaluationFinished { get; }
	}
}