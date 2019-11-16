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
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Application.Tournaments.Commands
{
	public class PublishTournamentCommand : IRequest
	{
		public long TournamentId { get; set; }

		public class Handler : IRequestHandler<PublishTournamentCommand>
		{
			private readonly ILogger<PublishTournamentCommand> logger;
			private readonly IRepository<Tournament> repository;
			private readonly ITimeService time;

			public Handler(ILogger<PublishTournamentCommand> logger,
				IRepository<Tournament> repository, ITimeService time)
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
					await repository.GetAsync(request.TournamentId, cancellationToken);

				if (tournament.State != TournamentState.Created)
				{
					throw new BadTournamentStateException(request.TournamentId,
						TournamentState.Created, tournament.State);
				}

				var targetState = tournament.Scope switch
				{
					TournamentScope.Deadline => TournamentState.Published,
					// avoid waiting for manual start, since submissions are accepted even in
					// running state
					TournamentScope.Ongoing => TournamentState.Running,
					_ => throw new ArgumentOutOfRangeException()
				};

				var updateDto = new PublishUpdateDto(time.Now, targetState);
				await repository.UpdateAsync(request.TournamentId, updateDto, cancellationToken);
				logger.TournamentStateChanged(request.TournamentId, updateDto.State);

				return Unit.Value;
			}

			public class PublishUpdateDto : TournamentStateUpdateDto
			{
				/// <inheritdoc />
				public PublishUpdateDto(DateTime published, TournamentState state)
					: base(state)
				{
					Published = published;
				}

				public DateTime Published { get; }
			}
		}
	}
}