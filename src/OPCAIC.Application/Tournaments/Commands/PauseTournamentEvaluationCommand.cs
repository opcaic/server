using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Tournaments.Commands
{
	public class PauseTournamentEvaluationCommand : IRequest
	{
		public long TournamentId { get; set; }

		public class Handler : IRequestHandler<PauseTournamentEvaluationCommand>
		{
			private readonly ILogger<PauseTournamentEvaluationCommand> logger;
			private readonly ITournamentRepository repository;

			public Handler(ILogger<PauseTournamentEvaluationCommand> logger,
				ITournamentRepository repository)
			{
				this.repository = repository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(PauseTournamentEvaluationCommand request, CancellationToken cancellationToken)
			{
				var tournament = await repository.FindByIdAsync(request.TournamentId, cancellationToken);

				if (tournament == null)
				{
					throw new NotFoundException(nameof(Tournament), request.TournamentId);
				}

				if (tournament.State != TournamentState.Running)
				{
					throw new BadTournamentStateException(request.TournamentId,
						nameof(TournamentState.Running), tournament.State.ToString());
				}

				var updateDto = new TournamentStateUpdateDto(TournamentState.Paused);
				await repository.UpdateAsync(request.TournamentId, updateDto, cancellationToken);
				logger.TournamentStateChanged(request.TournamentId, updateDto.State);

				return Unit.Value;
			}
		}
	}
}
