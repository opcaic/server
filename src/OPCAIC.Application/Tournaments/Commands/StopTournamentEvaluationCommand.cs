using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Tournaments.Commands
{
	public class StopTournamentEvaluationCommand : IRequest
	{
		public long TournamentId { get; set; }

		public class Handler : IRequestHandler<StopTournamentEvaluationCommand>
		{
			private readonly ILogger<StopTournamentEvaluationCommand> logger;
			private readonly ITournamentRepository repository;

			public Handler(ILogger<StopTournamentEvaluationCommand> logger,
				ITournamentRepository repository)
			{
				this.repository = repository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(StopTournamentEvaluationCommand request, CancellationToken cancellationToken)
			{
				var tournament = await repository.FindByIdAsync(request.TournamentId, cancellationToken);

				if (tournament == null)
				{
					throw new NotFoundException(nameof(Tournament), request.TournamentId);
				}

				if (tournament.State != TournamentState.Running)
				{
					throw new BadTournamentStateException(nameof(Tournament), request.TournamentId,
						nameof(TournamentState.Running), tournament.State.ToString());
				}
				if (tournament.Scope != TournamentScope.Ongoing)
				{
					throw new BadTournamentScopeException(nameof(Tournament), request.TournamentId,
						nameof(TournamentScope.Ongoing), tournament.Scope.ToString());
				}

				await repository.UpdateTournamentState(request.TournamentId,
					new TournamentStateUpdateDto { State = TournamentState.Stopped },
					cancellationToken);
				logger.TournamentStateChanged(request.TournamentId, TournamentState.Stopped);

				return Unit.Value;
			}
		}
	}
}
