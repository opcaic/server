using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.Base;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Submissions.Events;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Submissions.Commands
{
	public class SubmitSubmissionCommand : AuthenticatedRequest, IRequest<long>
	{
		public long TournamentId { get; set; }

		public Stream Archive { get; set; }

		public class Handler : IRequestHandler<SubmitSubmissionCommand, long>
		{
			private readonly IMediator mediator;
			private readonly ILogger<SubmitSubmissionCommand> logger;
			private readonly IRepository<Submission> repository;
			private readonly IStorageService storage;
			private readonly ITimeService time;
			private readonly ITournamentRepository tournamentRepository;
			private readonly IRepository<TournamentParticipation> tournamentParticipationsRepository;

			/// <inheritdoc />
			public Handler(IMediator mediator, ILogger<SubmitSubmissionCommand> logger,
				ITimeService time, IStorageService storage,
				ISubmissionRepository repository, ITournamentRepository tournamentRepository, IRepository<TournamentParticipation> tournamentParticipationsRepository)
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
						"This tournament currently does not accept submissions.");
				}

				var participation = await tournamentParticipationsRepository.FindAsync(p => 
					p.TournamentId == request.TournamentId &&
					p.UserId == request.RequestingUserId, cancellationToken);

				if (participation == null)
				{
					participation = new TournamentParticipation
					{
						TournamentId = request.TournamentId,
						UserId = request.RequestingUserId
					};

					tournamentParticipationsRepository.Add(participation);
				}

				// save db entity
				var submission = new Submission
				{
					TournamentId = request.TournamentId,
					AuthorId = request.RequestingUserId,
					Score = tournament.Format == TournamentFormat.Elo ? 1200 : 0,
					TournamentParticipation = participation
				};

				repository.Add(submission);
				await repository.SaveChangesAsync(cancellationToken);

				// save archive
				await using (var stream = storage.WriteSubmissionArchive(new SubmissionDtoBase()
				{
					Id = submission.Id,
					TournamentId = submission.TournamentId
				}))
				{
					// TODO: do we really want to permit cancellation here?
					// TODO: connect db transactions and filesystem transactions storage
					await request.Archive.CopyToAsync(stream, cancellationToken);
				}

				logger.SubmissionCreated(submission.Id, submission.TournamentId);
				await mediator.Publish(new SubmissionCreated(submission.Id), cancellationToken);

				return submission.Id;
			}

			public bool CanTournamentAcceptSubmissions(TournamentDetailDto tournament)
			{
				return tournament.State == TournamentState.Published &&
					(tournament.Deadline == null || tournament.Deadline > time.Now) ||
					tournament.State == TournamentState.Running &&
					tournament.Scope == TournamentScope.Ongoing;
			}
		}
	}
}