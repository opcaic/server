using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Submissions.Commands
{
	public class EnqueueValidationCommand : IRequest<Guid>
	{
		public EnqueueValidationCommand(long submissionId)
		{
			SubmissionId = submissionId;
		}

		public long SubmissionId { get; }

		public class Handler : IRequestHandler<EnqueueValidationCommand, Guid>
		{
			private readonly ILogger<EnqueueValidationCommand> logger;
			private readonly ISubmissionValidationRepository repository;
			private readonly ISubmissionRepository submissionRepository;

			public Handler(ISubmissionValidationRepository repository,
				ISubmissionRepository submissionRepository,
				ILogger<EnqueueValidationCommand> logger)
			{
				this.repository = repository;
				this.submissionRepository = submissionRepository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<Guid> Handle(EnqueueValidationCommand request,
				CancellationToken cancellationToken)
			{
				var validation = new NewSubmissionValidationDto
				{
					SubmissionId = request.SubmissionId, JobId = Guid.NewGuid()
				};
				var id = await repository.CreateAsync(validation, cancellationToken);
				await submissionRepository.UpdateAsync(request.SubmissionId,
					new UpdateValidationStateDto(SubmissionValidationState.Queued),
					cancellationToken);
				logger.SubmissionValidationQueued(id, request.SubmissionId, validation.JobId);

				return validation.JobId;
			}
		}
	}
}