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
	public class UnpauseTournamentEvaluationCommand : IRequest
	{
		public long TournamentId { get; set; }

		public class Handler : IRequestHandler<UnpauseTournamentEvaluationCommand>
		{
			private readonly ILogger<UnpauseTournamentEvaluationCommand> logger;
			private readonly IRepository<Tournament> repository;

			public Handler(ILogger<UnpauseTournamentEvaluationCommand> logger,
				IRepository<Tournament> repository)
			{
				this.repository = repository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(UnpauseTournamentEvaluationCommand request,
				CancellationToken cancellationToken)
			{
				var tournament =
					await repository.GetAsync(request.TournamentId, cancellationToken);

				if (tournament.State != TournamentState.Paused)
				{
					throw new BadTournamentStateException(request.TournamentId,
						nameof(TournamentState.Paused), tournament.State.ToString());
				}

				// do not use TournamentStartedUpdateDto in order to not update EvaluationStarted
				var dto = new TournamentStateUpdateDto(TournamentState.Running);
				await repository.UpdateAsync(request.TournamentId, dto, cancellationToken);
				logger.TournamentStateChanged(request.TournamentId, dto.State);

				return Unit.Value;
			}
		}
	}
}