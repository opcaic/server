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
	public class StartTournamentEvaluationCommand : IRequest
	{
		public long TournamentId { get; set; }

		public class Handler : IRequestHandler<StartTournamentEvaluationCommand>
		{
			private readonly ILogger<StartTournamentEvaluationCommand> logger;
			private readonly ITournamentRepository repository;

			public Handler(ILogger<StartTournamentEvaluationCommand> logger,
				ITournamentRepository repository)
			{
				this.repository = repository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(StartTournamentEvaluationCommand request, CancellationToken cancellationToken)
			{
				var tournament = await repository.FindByIdAsync(request.TournamentId, cancellationToken);

				if (tournament == null)
				{
					throw new NotFoundException(nameof(Tournament), request.TournamentId);
				}

				if (tournament.State != TournamentState.Published)
				{
					throw new BadTournamentStateException(nameof(Tournament), request.TournamentId,
						nameof(TournamentState.Published), tournament.State.ToString());
				}

				await repository.UpdateTournamentState(request.TournamentId,
					new TournamentStateUpdateDto { State = TournamentState.Running },
					cancellationToken);
				logger.TournamentStateChanged(request.TournamentId, TournamentState.Running);

				return Unit.Value;
			}
		}
	}
}
