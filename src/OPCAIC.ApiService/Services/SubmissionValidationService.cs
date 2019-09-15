using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.Models.SubmissionValidations;
using OPCAIC.ApiService.Security;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.ApiService.Services
{
	public class SubmissionValidationService : ISubmissionValidationService
	{
		private readonly IMapper mapper;
		private readonly ISubmissionValidationRepository repository;
		private readonly IWorkerService workerService;
		private readonly ILogger<SubmissionValidationService> logger;
		private readonly ILogStorageService logStorage;

		public SubmissionValidationService(ISubmissionValidationRepository repository, IWorkerService workerService,
			ILogger<SubmissionValidationService> logger, IMapper mapper, ILogStorageService logStorage)
		{
			this.repository = repository;
			this.workerService = workerService;
			this.logger = logger;
			this.mapper = mapper;
			this.logStorage = logStorage;
		}

		/// <inheritdoc />
		public async Task EnqueueValidationAsync(long submissionId,
			CancellationToken cancellationToken)
		{
			var validation = new NewSubmissionValidationDto { SubmissionId = submissionId, JobId = Guid.NewGuid() };
			var id = await repository.CreateAsync(validation, cancellationToken);
			logger.SubmissionValidationQueued(id, submissionId, validation.JobId);
		}

		public SubmissionValidationRequest CreateRequest(SubmissionValidationRequestDataDto data)
		{
			return new SubmissionValidationRequest
			{
				JobId = data.JobId,
				SubmissionId = data.SubmissionId,
				ValidationId = data.Id,
				Configuration = data.TournamentConfiguration,
				Game = data.GameKey,
				AdditionalFilesUri = workerService.GetAdditionalFilesUrl(data.TournamentId),
				AccessToken = CreateAccessToken(data.SubmissionId, data.Id, data.TournamentId)
			};
		}

		public Task UpdateFromMessage(SubmissionValidationResult result)
		{
			var dto = mapper.Map<UpdateSubmissionValidationDto>(result);
			dto.Executed = DateTime.Now;
			logger.SubmissionValidationUpdated(result.JobId, dto);
			return repository.UpdateFromJobAsync(result.JobId, dto, CancellationToken.None);
		}

		/// <inheritdoc />
		public Task OnValidationRequestExpired(Guid jobId)
		{
			logger.SubmissionValidationExpired(jobId);
			return repository.UpdateJobStateAsync(jobId,
				new JobStateUpdateDto {State = WorkerJobState.Waiting}, CancellationToken.None);
		}

		/// <inheritdoc />
		public async Task<SubmissionValidationDetailModel> GetByIdAsync(long id, CancellationToken cancellationToken)
		{
			var dto = await repository.FindByIdAsync(id, cancellationToken);
			if (dto == null)
			{
				throw new NotFoundException(nameof(SubmissionValidation), id);
			}

			var logs =
				logStorage.GetSubmissionValidationLogs(
					mapper.Map<SubmissionValidationStorageDto>(dto));

			var model = mapper.Map<SubmissionValidationDetailModel>(dto);
			mapper.Map(logs, model);
			return model;
		}

		private string CreateAccessToken(long submissionId, long validationId, long tournamentId)
		{
			var workerIdentity = new ClaimsIdentity();

			workerIdentity.AddClaim(new Claim(WorkerClaimTypes.SubmissionId,
				submissionId.ToString()));
			workerIdentity.AddClaim(new Claim(WorkerClaimTypes.ValidationId,
				validationId.ToString()));
			workerIdentity.AddClaim(new Claim(WorkerClaimTypes.TournamentId,
				tournamentId.ToString()));

			return workerService.GenerateWorkerToken(workerIdentity);
		}
	}
}