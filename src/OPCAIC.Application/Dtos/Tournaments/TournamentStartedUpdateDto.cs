using System;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Tournaments
{
	public class TournamentStartedUpdateDto : TournamentStateUpdateDto
	{
		public TournamentStartedUpdateDto(DateTime evaluationStarted)
			: base(TournamentState.Running)
		{
			EvaluationStarted = evaluationStarted;
		}

		public DateTime EvaluationStarted { get; }
	}
}