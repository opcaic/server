using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Tournaments.Commands
{
	public class PublishTournamentCommand : IRequest
	{
		public long TournamentId { get; set; }

		public class Handler : IRequestHandler<PublishTournamentCommand>
		{
			private readonly ILogger<PublishTournamentCommand> logger;
			private readonly ITournamentRepository repository;
			private readonly ITimeService time;

			public Handler(ILogger<PublishTournamentCommand> logger,
				ITournamentRepository repository, ITimeService time)
			{
				this.repository = repository;
				this.time = time;
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
					throw new BadTournamentStateException(request.TournamentId,
						nameof(TournamentState.Created), tournament.State.ToString());
				}

				var updateDto = new PublishUpdateDto(time.Now);
				await repository.UpdateAsync(request.TournamentId, updateDto, cancellationToken);
				logger.TournamentStateChanged(request.TournamentId, updateDto.State);

				return Unit.Value;
			}

			public class PublishUpdateDto : TournamentStateUpdateDto
			{
				/// <inheritdoc />
				public PublishUpdateDto(DateTime published)
					: base(TournamentState.Published)
				{
					Published = published;
				}

				public DateTime Published { get; }
			}
		}
	}
}