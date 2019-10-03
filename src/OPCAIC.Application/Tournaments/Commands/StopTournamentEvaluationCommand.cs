using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Application.Tournaments.Commands
{
	public class StopTournamentEvaluationCommand : IRequest
	{
		public long TournamentId { get; set; }

		public class Handler : IRequestHandler<StopTournamentEvaluationCommand>
		{
			private readonly ILogger<StopTournamentEvaluationCommand> logger;
			private readonly IRepository<Tournament> repository;

			public Handler(ILogger<StopTournamentEvaluationCommand> logger,
				IRepository<Tournament> repository)
			{
				this.repository = repository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(StopTournamentEvaluationCommand request, CancellationToken cancellationToken)
			{
				var tournament = await repository.GetAsync(request.TournamentId, cancellationToken);

				if (tournament.State != TournamentState.Running && tournament.State != TournamentState.Paused)
				{
					throw new BadTournamentStateException(request.TournamentId,
						nameof(TournamentState.Running), tournament.State.ToString());
				}
				if (tournament.Scope != TournamentScope.Ongoing)
				{
					throw new BadTournamentScopeException(request.TournamentId,
						nameof(TournamentScope.Ongoing), tournament.Scope.ToString());
				}

				// Will be transitioned to finished by TournamentProcessor when all matches finish
				var dto = new TournamentStateUpdateDto(TournamentState.WaitingForFinish);
				await repository.UpdateAsync(request.TournamentId, dto, cancellationToken);
				logger.TournamentStateChanged(request.TournamentId, dto.State);

				return Unit.Value;
			}
		}
	}
}
