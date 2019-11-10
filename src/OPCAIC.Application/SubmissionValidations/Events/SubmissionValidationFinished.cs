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
using OPCAIC.Application.Submissions.Events;
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
			private readonly ISubmissionValidationRepository repository;
			private readonly ISubmissionRepository submissionRepository;

			public Handler(ISubmissionValidationRepository repository,
				ILogger<SubmissionValidationFinished> logger,
				ISubmissionRepository submissionRepository, IMediator mediator)
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
							.First().Id
					}, cancellationToken);

				if (data.ValidationId != data.LastValidationId)
				{
					return; // newer validation exists
				}

				var validationState = SelectValidationState(notification);
				await submissionRepository.UpdateAsync(data.SubmissionId,
					new UpdateValidationStateDto(validationState), cancellationToken);

				await mediator.Publish(
					new SubmissionValidationStateChanged(data.SubmissionId, data.TournamentId,
						data.ValidationId, data.UserId, validationState), cancellationToken);
			}

			public SubmissionValidationState SelectValidationState(
				SubmissionValidationFinished notification)
			{
				if (notification.State == WorkerJobState.Cancelled)
				{
					return SubmissionValidationState.Cancelled;
				}

				switch (notification.ValidatorResult)
				{
					case EntryPointResult.Success:
						return SubmissionValidationState.Valid;

					case EntryPointResult.UserError:
						return SubmissionValidationState.Invalid;

					case EntryPointResult.Cancelled:
						return SubmissionValidationState.Cancelled;

					case EntryPointResult.NotExecuted: // validation ended in earlier stage
					case EntryPointResult.ModuleError:
					case EntryPointResult.PlatformError:
						return SubmissionValidationState.Error;

					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			public class Data
			{
				public long TournamentId { get; set; }
				public long UserId { get; set; }
				public long SubmissionId { get; set; }
				public long ValidationId { get; set; }
				public long LastValidationId { get; set; }
			}
		}
	}
}