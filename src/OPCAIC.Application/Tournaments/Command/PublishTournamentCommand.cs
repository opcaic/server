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

namespace OPCAIC.Application.Tournaments.Command
{
	public class PublishTournamentCommand : IRequest
	{
		public long TournamentId { get; set; }

		public class Handler : IRequestHandler<PublishTournamentCommand>
		{
			private readonly ILogger<PublishTournamentCommand> logger;
			private readonly ITournamentRepository repository;

			public Handler(ILogger<PublishTournamentCommand> logger,
				ITournamentRepository repository)
			{
				this.repository = repository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(PublishTournamentCommand request,
				CancellationToken cancellationToken)
			{
				var tournament =
					await repository.FindByIdAsync(request.TournamentId, cancellationToken);

				if (tournament == null)
				{
					throw new NotFoundException(nameof(Tournament), request.TournamentId);
				}

				if (tournament.State != TournamentState.Created)
				{
					throw new BadTournamentStateException(nameof(Tournament), request.TournamentId,
						nameof(TournamentState.Created), tournament.State.ToString());
				}

				await repository.UpdateTournamentState(request.TournamentId,
					new TournamentStateUpdateDto {State = TournamentState.Published},
					cancellationToken);
				logger.TournamentStateChanged(request.TournamentId, TournamentState.Published);

				return Unit.Value;
			}
		}
	}
}