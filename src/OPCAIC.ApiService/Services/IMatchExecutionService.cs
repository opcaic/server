using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Security;
using OPCAIC.Broker;
using OPCAIC.Infrastructure.Dtos.MatchExecutions;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Messaging.Messages;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Services
{
	public interface IMatchExecutionService
	{
		Task EnqueueExecutionAsync(long matchId, CancellationToken cancellationToken);
		Task UpdateFromMessage(MatchExecutionResult result);
	}


	internal class MatchExecutionService : IMatchExecutionService
	{
		private readonly IBroker broker;
		private readonly ILogger<MatchExecutionService> logger;
		private readonly IMapper mapper;
		private readonly IMatchRepository matchRepository;
		private readonly IMatchExecutionRepository repository;
		private readonly ITournamentRepository tournamentRepository;
		private readonly IWorkerService workerService;

		public MatchExecutionService(IBroker broker, ILogger<MatchExecutionService> logger,
			IMatchExecutionRepository repository, IMatchRepository matchRepository,
			IWorkerService workerService, ITournamentRepository tournamentRepository, IMapper mapper)
		{
			this.broker = broker;
			this.logger = logger;
			this.repository = repository;
			this.matchRepository = matchRepository;
			this.workerService = workerService;
			this.tournamentRepository = tournamentRepository;
			this.mapper = mapper;
		}

		/// <inheritdoc />
		public async Task EnqueueExecutionAsync(long matchId, CancellationToken cancellationToken)
		{
			var match = await matchRepository.FindByIdAsync(matchId, cancellationToken);
			var tournament =
				await tournamentRepository.FindByIdAsync(match.Tournament.Id, cancellationToken);

			var execution = new NewMatchExecutionDto {MatchId = matchId, JobId = Guid.NewGuid()};
			var executionId = await repository.CreateAsync(execution, cancellationToken);

			var message = new MatchExecutionRequest
			{
				JobId = execution.JobId,
				ExecutionId = executionId,
				Bots =
					match.Submissions.Select(s => new BotInfo {SubmissionId = s.Id}).ToList(),
				Configuration = tournament.Configuration,
				Game = tournament.Game.key,
				AdditionalFilesUri = workerService.GetAdditionalFilesUrl(tournament.Id),
				AccessToken = CreateAccessToken(match.Submissions, tournament.Id, executionId)
			};

			logger.LogInformation(LoggingEvents.MatchQeueuExecution,
				$"Queueing execution of match {{{LoggingTags.MatchId}}} in tournament {{{LoggingTags.TournamentId}}} and game {{{LoggingTags.Game}}} as job {{{LoggingTags.JobId}}}.",
				matchId, tournament.Id, tournament.Game.key, message.JobId);

			await broker.EnqueueWork(message);
		}

		/// <inheritdoc />
		public Task UpdateFromMessage(MatchExecutionResult result)
		{
			var dto = mapper.Map<UpdateMatchExecutionDto>(result);
			dto.Executed = DateTime.Now;
			return repository.UpdateFromJobAsync(result.JobId, dto, CancellationToken.None);
		}

		private string CreateAccessToken(IEnumerable<SubmissionReferenceDto> submissions,
			long tournamentId, long executionId)
		{
			var workerIdentity = new ClaimsIdentity();

			foreach (var sub in submissions)
			{
				workerIdentity.AddClaim(new Claim(WorkerClaimTypes.SubmissionId,
					sub.Id.ToString()));
			}

			workerIdentity.AddClaim(new Claim(WorkerClaimTypes.ExecutionId,
				executionId.ToString()));
			workerIdentity.AddClaim(new Claim(WorkerClaimTypes.TournamentId,
				tournamentId.ToString()));

			return workerService.GenerateWorkerToken(workerIdentity);
		}
	}
}