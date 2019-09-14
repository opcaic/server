using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Models.Matches;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.ApiService.Security;
using OPCAIC.Broker;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.MatchExecutions;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Messaging.Messages;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Services
{
	internal class MatchExecutionService : IMatchExecutionService
	{
		private readonly ILogger<MatchExecutionService> logger;
		private readonly IMapper mapper;
		private readonly IMatchExecutionRepository repository;
		private readonly IWorkerService workerService;
		private readonly ILogStorageService logStorage;
		private readonly ISubmissionScoreService scoreService;

		public MatchExecutionService(ILogger<MatchExecutionService> logger,
			IMatchExecutionRepository repository,
			IWorkerService workerService, 
			ISubmissionScoreService scoreService,
			IMapper mapper, ILogStorageService logStorage)
		{
			this.logger = logger;
			this.repository = repository;
			this.workerService = workerService;
			this.scoreService = scoreService;
			this.mapper = mapper;
			this.logStorage = logStorage;
		}

		/// <inheritdoc />
		public async Task EnqueueExecutionAsync(long matchId, CancellationToken cancellationToken)
		{
			var execution = new NewMatchExecutionDto {MatchId = matchId, JobId = Guid.NewGuid()};
			var id = await repository.CreateAsync(execution, cancellationToken);
			logger.MatchExecutionQueued(id, matchId, execution.JobId);
		}

		/// <inheritdoc />
		public async Task<MatchExecutionDetailModel> GetByIdAsync(long id, CancellationToken cancellationToken)
		{
			var dto = await repository.FindByIdAsync(id, cancellationToken);
			if (dto == null)
			{
				throw new NotFoundException(nameof(MatchExecution), id);
			}

			var logs =
				logStorage.GetMatchExecutionLogs(
					mapper.Map<MatchExecutionStorageDto>(dto));
			var model = mapper.Map<MatchExecutionDetailModel>(dto);

			for (var i = 0; i < model.BotResults.Count; i++)
			{
				var detail = mapper.Map<SubmissionMatchResultDetailModel>(model.BotResults[i]);

				// the log collection may be "too short"
				if (i < logs.CompilerLogs.Count)
				{
					detail.CompilerLog = logs.CompilerLogs[i];
				}
			}

			model.ExecutorLog = logs.ExecutorLog;
			return model;
		}

		public MatchExecutionRequest CreateRequest(MatchExecutionRequestDataDto data)
		{
			return new MatchExecutionRequest
			{
				JobId = data.JobId,
				ExecutionId = data.Id,
				Configuration = data.TournamentConfiguration,
				Game = data.GameKey,
				Bots = data.SubmissionIds.Select(id => new BotInfo {SubmissionId = id}).ToList(),
				AdditionalFilesUri = workerService.GetAdditionalFilesUrl(data.TournamentId),
				AccessToken = CreateAccessToken(data.SubmissionIds, data.TournamentId, data.Id)
			};
		}

		/// <inheritdoc />
		public Task UpdateFromMessage(MatchExecutionResult result)
		{
			var dto = mapper.Map<UpdateMatchExecutionDto>(result);
			dto.Executed = DateTime.Now;
			scoreService.UpdateSubmissionsScore(result, new CancellationToken());
			logger.MatchExecutionUpdated(result.JobId, dto);
			return repository.UpdateFromJobAsync(result.JobId, dto, CancellationToken.None);
		}

		/// <inheritdoc />
		public Task OnExecutionRequestExpired(Guid jobId)
		{
			logger.MatchExecutionExpired(jobId);
			return repository.UpdateJobStateAsync(jobId,
				new JobStateUpdateDto {State = WorkerJobState.Waiting}, CancellationToken.None);
		}

		private string CreateAccessToken(IEnumerable<long> submissions,
			long tournamentId, long executionId)
		{
			var workerIdentity = new ClaimsIdentity();

			foreach (var sub in submissions)
			{
				workerIdentity.AddClaim(new Claim(WorkerClaimTypes.SubmissionId,
					sub.ToString()));
			}

			workerIdentity.AddClaim(new Claim(WorkerClaimTypes.ExecutionId,
				executionId.ToString()));
			workerIdentity.AddClaim(new Claim(WorkerClaimTypes.TournamentId,
				tournamentId.ToString()));

			return workerService.GenerateWorkerToken(workerIdentity);
		}
	}
}