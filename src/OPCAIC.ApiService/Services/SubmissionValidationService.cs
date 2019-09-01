﻿using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Security;
using OPCAIC.Broker;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Messaging.Messages;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Services
{
	public class SubmissionValidationService : ISubmissionValidationService
	{
		private readonly IBroker broker;
		private readonly IMapper mapper;
		private readonly ILogger<SubmissionValidationService> logger;
		private readonly ISubmissionValidationRepository repository;
		private readonly ISubmissionRepository submissionRepository;
		private readonly ITournamentRepository tournamentRepository;
		private readonly IWorkerService workerService;

		public SubmissionValidationService(ISubmissionValidationRepository repository,
			IBroker broker, ISubmissionRepository submissionRepository,
			ITournamentRepository tournamentRepository, IWorkerService workerService,
			ILogger<SubmissionValidationService> logger, IMapper mapper)
		{
			this.repository = repository;
			this.broker = broker;
			this.submissionRepository = submissionRepository;
			this.tournamentRepository = tournamentRepository;
			this.workerService = workerService;
			this.logger = logger;
			this.mapper = mapper;
		}

		/// <inheritdoc />
		public async Task EnqueueValidationAsync(long submissionId,
			CancellationToken cancellationToken)
		{
			var submission =
				await submissionRepository.FindByIdAsync(submissionId, cancellationToken);
			var tournament =
				await tournamentRepository.FindByIdAsync(submission.Tournament.Id,
					cancellationToken);
			var validation = new NewSubmissionValidationDto
			{
				SubmissionId = submissionId, JobId = Guid.NewGuid()
			};

			var validationId = await repository.CreateAsync(validation, cancellationToken);

			var m = new SubmissionValidationRequest
			{
				JobId = validation.JobId,
				SubmissionId = submissionId,
				ValidationId = validationId,
				Configuration = tournament.Configuration,
				Game = tournament.Game.key,
				AdditionalFilesUri = workerService.GetAdditionalFilesUrl(tournament.Id),
				AccessToken = CreateAccessToken(submissionId, validationId, tournament.Id)
			};

			LogValidationQueued(m, tournament.Game.key, tournament.Id);
			await broker.EnqueueWork(m);
		}

		private void LogValidationQueued(SubmissionValidationRequest request, string game,
			long tournamentId)
		{
			logger.LogInformation(LoggingEvents.SubmissionQueueValidate,
				$"Queueing validation of submission {{{LoggingTags.SubmissionId}}} in tournament {{{LoggingTags.TournamentId}}} and game {{{LoggingTags.Game}}} as job {{{LoggingTags.JobId}}}.",
				request.SubmissionId, tournamentId, game, request.JobId);
		}

		public Task UpdateFromMessage(SubmissionValidationResult result)
		{
			var dto = mapper.Map<UpdateSubmissionValidationDto>(result);
			dto.Executed = DateTime.Now;
			return repository.UpdateFromJobAsync(result.JobId, dto, CancellationToken.None);
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