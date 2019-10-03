using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Specifications;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Application.Tournaments.Commands
{
	public class StartTournamentEvaluationCommand : IRequest
	{
		public long TournamentId { get; set; }

		public class Handler : IRequestHandler<StartTournamentEvaluationCommand>
		{
			private readonly ILogger<StartTournamentEvaluationCommand> logger;
			private readonly IRepository<Tournament> repository;
			private readonly ITimeService time;

			public Handler(ILogger<StartTournamentEvaluationCommand> logger,
				IRepository<Tournament> repository, ITimeService time)
			{
				this.repository = repository;
				this.time = time;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(StartTournamentEvaluationCommand request,
				CancellationToken cancellationToken)
			{
				var tournament =
					await repository.GetAsync(request.TournamentId, cancellationToken);

				if (tournament.State != TournamentState.Published)
				{
					throw new BadTournamentStateException(request.TournamentId,
						nameof(TournamentState.Published), tournament.State.ToString());
				}

				var dto = new TournamentStartedUpdateDto(time.Now);
				await repository.UpdateAsync(request.TournamentId, dto, cancellationToken);
				logger.TournamentStateChanged(request.TournamentId, dto.State);

				return Unit.Value;
			}
		}
	}
}