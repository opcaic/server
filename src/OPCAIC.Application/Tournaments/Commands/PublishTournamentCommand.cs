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
			private readonly IMediator mediator;

			public Handler(ILogger<PublishTournamentCommand> logger,
				IRepository<Tournament> repository, ITimeService time, IMediator mediator)
			{
				this.repository = repository;
				this.time = time;
				this.mediator = mediator;
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

				var updateDto = new PublishUpdateDto(time.Now);
				await repository.UpdateAsync(request.TournamentId, updateDto, cancellationToken);
				logger.TournamentStateChanged(request.TournamentId, updateDto.State);

				if (tournament.Scope == TournamentScope.Ongoing)
				{
					await mediator.Send(new StartTournamentEvaluationCommand
					{
						TournamentId = request.TournamentId
					}, cancellationToken);
				}

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