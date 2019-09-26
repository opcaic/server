using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Submissions.Events;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Submissions.Commands
{
	public class SubmitSubmissionCommand : PublicRequest, IRequest<long>
	{
		public long TournamentId { get; set; }

		public Stream Archive { get; set; }

		public class Handler : IRequestHandler<SubmitSubmissionCommand, long>
		{
			private readonly IMediator mediator;
			private readonly ILogger<SubmitSubmissionCommand> logger;
			private readonly ISubmissionRepository repository;
			private readonly IStorageService storage;
			private readonly ITimeService time;
			private readonly ITournamentRepository tournamentRepository;
			private readonly ITournamentParticipationsRepository tournamentParticipationsRepository;

			/// <inheritdoc />
			public Handler(IMediator mediator, ILogger<SubmitSubmissionCommand> logger,
				ITimeService time, IStorageService storage,
				ISubmissionRepository repository, ITournamentRepository tournamentRepository, ITournamentParticipationsRepository tournamentParticipationsRepository)
			{
				this.storage = storage;
				this.repository = repository;
				this.tournamentRepository = tournamentRepository;
				this.time = time;
				this.logger = logger;
				this.mediator = mediator;
				this.tournamentParticipationsRepository = tournamentParticipationsRepository;
			}

			/// <inheritdoc />
			public async Task<long> Handle(SubmitSubmissionCommand request,
				CancellationToken cancellationToken)
			{
				if (request.RequestingUserId == null)
				{
					// TODO: This should never happen with the authentication at the WebAPI side, maybe introduce another interface where the RequestingUserId is not nullable
					throw new BusinessException(ValidationErrorCodes.GenericError,
						"User not logged in");
				}

				// check whether the tournament can still accept submissions
				var tournament =
					await tournamentRepository.FindByIdAsync(request.TournamentId,
						cancellationToken);

				if (!CanTournamentAcceptSubmissions(tournament))
				{
					if (tournament.Deadline.HasValue && tournament.Deadline < time.Now)
					{
						throw new BusinessException(ValidationErrorCodes.TournamentDeadlinePassed,
							"Deadline for tournament registration has passed.");
					}

					throw new BusinessException(
						ValidationErrorCodes.TournamentDoesNotAcceptSubmission,
						"This tournament does not accept submissions anymore.");
				}

				if (!await tournamentParticipationsRepository.ExistsAsync(p
					=> p.TournamentId == request.TournamentId &&
					p.UserId == request.RequestingUserId, cancellationToken))
				{
					await tournamentParticipationsRepository.CreateAsync(
						new TournamentParticipation
						{
							TournamentId = request.TournamentId,
							UserId = request.RequestingUserId.Value
						}, cancellationToken);
				}

				// save db entity
				var dto = new NewSubmissionDto
				{
					TournamentId = request.TournamentId,
					AuthorId = request.RequestingUserId.Value,
					Score = tournament.Format == TournamentFormat.Elo ? 1200 : 0
				};

				var id = await repository.CreateAsync(dto, cancellationToken);
				var storeDto =
					await repository.FindSubmissionForStorageAsync(id, cancellationToken);

				// save archive
				using (var stream = storage.WriteSubmissionArchive(storeDto))
				{
					// TODO: do we really want to permit cancellation here?
					// TODO: connect db transactions and filesystem transactions storage
					await request.Archive.CopyToAsync(stream, cancellationToken);
				}

				logger.SubmissionCreated(id, dto);
				await mediator.Publish(new SubmissionCreated(id), cancellationToken);

				return id;
			}

			public static bool CanTournamentAcceptSubmissions(TournamentDetailDto tournament)
			{
				return tournament.State == TournamentState.Published &&
					(tournament.Deadline == null || tournament.Deadline > DateTime.Now) ||
					tournament.State == TournamentState.Running &&
					tournament.Scope == TournamentScope.Ongoing;
			}
		}
	}
}