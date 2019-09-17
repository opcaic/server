using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.TournamentParticipations;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.SubmissionValidations.Events
{
	public class SubmissionValidationFinished : INotification
	{
		public Guid JobId { get; set; }
		public WorkerJobState State { get; set; }
		public EntryPointResult CheckerResult { get; set; }
		public EntryPointResult CompilerResult { get; set; }
		public EntryPointResult ValidatorResult { get; set; }
		public DateTime Executed { get; set; }

		public class Handler : INotificationHandler<SubmissionValidationFinished>
		{
			private readonly ISubmissionValidationRepository repository;
			private readonly ITournamentParticipationsRepository participationsRepository;
			private readonly ILogger<SubmissionValidationFinished> logger;

			public Handler(ISubmissionValidationRepository repository, ILogger<SubmissionValidationFinished> logger, ITournamentParticipationsRepository participationsRepository)
			{
				this.repository = repository;
				this.logger = logger;
				this.participationsRepository = participationsRepository;
			}

			/// <inheritdoc />
			public async Task Handle(SubmissionValidationFinished notification, CancellationToken cancellationToken)
			{
				// update in database
				await repository.UpdateFromJobAsync(notification.JobId, notification, cancellationToken);
				logger.SubmissionValidationUpdated(notification);

				if (notification.ValidatorResult != EntryPointResult.Success)
				{
					return;
				}

				// check whether this is last validation of the submission
				var data = await repository.QueryAsync(
					q => q.Where(v => v.JobId == notification.JobId).Select(v => new
					{
						TournamentId = v.Submission.TournamentId,
						UserId = v.Submission.AuthorId,
						SubmissionId = v.SubmissionId,
						LastSubmissionId = v.Submission.TournamentParticipation.Submissions
							.OrderByDescending(s => s.Created).First().Id,
						ValidationId = v.Id,
						LastValidationId = v.Submission.Validations.OrderByDescending(vv => vv.Created).First().Id
					}), cancellationToken);

				if (data.ValidationId != data.LastValidationId)
				{
					return; // newer validation exists
				}

				if (data.SubmissionId != data.LastSubmissionId)
				{
					return; // newer submission exists
				}

				await participationsRepository.SetActiveSubmission(data.TournamentId, data.UserId,
					new UpdateTournamentParticipationDto
					{
						ActiveSubmissionId = data.SubmissionId
					}, cancellationToken);
			}
		}
	}
}