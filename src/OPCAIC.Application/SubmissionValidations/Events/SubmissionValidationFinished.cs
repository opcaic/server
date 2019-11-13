using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure.Events;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Submissions.Events;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.SubmissionValidations.Events
{
	public class SubmissionValidationFinished : SubmissionValidationEvent
	{
		public WorkerJobState State { get; set; }
		public EntryPointResult CheckerResult { get; set; }
		public EntryPointResult CompilerResult { get; set; }
		public EntryPointResult ValidatorResult { get; set; }
		public DateTime Executed { get; set; }
		public Exception Exception { get; set; }

		public class Handler : INotificationHandler<SubmissionValidationFinished>
		{
			private readonly ILogger<SubmissionValidationFinished> logger;
			private readonly IMediator mediator;
			private readonly IRepository<SubmissionValidation> repository;
			private readonly IRepository<Submission> submissionRepository;

			public Handler(IRepository<SubmissionValidation> repository,
				ILogger<SubmissionValidationFinished> logger,
				IRepository<Submission> submissionRepository, IMediator mediator)
			{
				this.repository = repository;
				this.logger = logger;
				this.submissionRepository = submissionRepository;
				this.mediator = mediator;
			}

			/// <inheritdoc />
			public async Task Handle(SubmissionValidationFinished notification,
				CancellationToken cancellationToken)
			{
				// update in database
				await repository.UpdateAsync(v => v.JobId == notification.JobId, notification,
					cancellationToken);

				logger.SubmissionValidationUpdated(notification);

				var data = await repository.GetAsync(v => v.JobId == notification.JobId, v
					=> new Data
					{
						TournamentId = v.Submission.TournamentId,
						UserId = v.Submission.AuthorId,
						SubmissionId = v.SubmissionId,
						ValidationId = v.Id,
						LastValidationId = v.Submission.Validations
							.OrderByDescending(vv => vv.Created)
							.First().Id,
						TournamentName = v.Submission.Tournament.Name
					}, cancellationToken);

				if (data.ValidationId != data.LastValidationId)
				{
					return; // newer validation exists
				}

				var validationState = CalculateValidationState(notification.State, notification.ValidatorResult);
				await submissionRepository.UpdateAsync(data.SubmissionId,
					new UpdateValidationStateDto(validationState), cancellationToken);

				await mediator.Publish(
					new SubmissionValidationStateChanged(data.SubmissionId, data.TournamentId,
						data.ValidationId, data.UserId, data.TournamentName, validationState), cancellationToken);
			}

			public static SubmissionValidationState CalculateValidationState(WorkerJobState state, EntryPointResult validatorResult) => state switch
			{
				WorkerJobState.Waiting => SubmissionValidationState.Queued,
				WorkerJobState.Scheduled => SubmissionValidationState.Queued,
				WorkerJobState.Cancelled => SubmissionValidationState.Cancelled,
				WorkerJobState.Finished => validatorResult switch
				{
					EntryPointResult.Success => SubmissionValidationState.Valid,
					EntryPointResult.UserError => SubmissionValidationState.Invalid,
					_ => SubmissionValidationState.Error
				},
				WorkerJobState.Blocked => SubmissionValidationState.Cancelled,
				_ => throw new ArgumentOutOfRangeException()
			};

			public class Data
			{
				public long TournamentId { get; set; }
				public long UserId { get; set; }
				public long SubmissionId { get; set; }
				public long ValidationId { get; set; }
				public long LastValidationId { get; set; }
				public string TournamentName { get; set; }
			}
		}
	}
}