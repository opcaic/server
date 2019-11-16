using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Domain.Entities;
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
				ILogger<EnqueueValidationCommand> logger, IMapper mapper)
			{
				this.repository = repository;
				this.submissionRepository = submissionRepository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<Guid> Handle(EnqueueValidationCommand request,
				CancellationToken cancellationToken)
			{
				var validation = new SubmissionValidation
				{
					SubmissionId = request.SubmissionId, JobId = Guid.NewGuid()
				};

				await repository.CreateAsync(validation, cancellationToken);

				var submission =
					await submissionRepository.GetAsync(request.SubmissionId, cancellationToken);

				submission.LastValidation = validation;
				submission.ValidationState = SubmissionValidationState.Queued;

				await submissionRepository.SaveChangesAsync(cancellationToken);

				logger.SubmissionValidationQueued(validation.Id, request.SubmissionId, validation.JobId);
				return validation.JobId;
			}
		}
	}
}