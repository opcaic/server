using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Security;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Application.MatchExecutions.Events;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.SubmissionValidations.Events;
using OPCAIC.Broker;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.ApiService.Services
{
	public class BrokerReactor : HostedJob
	{
		private const int maxBrokerItems = 50;
		private readonly IBroker broker;
		private readonly IMapper mapper;
		private readonly ITimeService time;

		public BrokerReactor(IBroker broker, IServiceScopeFactory scopeFactory,
			ILogger<BrokerReactor> logger, IMapper mapper, ITimeService time)
			: base(scopeFactory, logger, TimeSpan.FromSeconds(1))
		{
			this.broker = broker;
			this.mapper = mapper;
			this.time = time;

			broker.RegisterHandler<MatchExecutionResult>(
				msg => Task.Run(() => ScopeExecute((sp, ct) => OnMatchExecuted(sp, msg, ct))));
			broker.RegisterHandler<SubmissionValidationResult>(
				msg => Task.Run(()
					=> ScopeExecute((sp, ct) => OnSubmissionValidated(sp, msg, ct))));

			broker.MessageExpired += (_, msg)
				=> Task.Run(() => ScopeExecute((sp, ct) => OnTaskExpired(sp, msg, ct)));
		}


		protected override async Task Init(IServiceProvider services, CancellationToken cancellationToken)
		{
			var executionRepository = services.GetRequiredService<IRepository<MatchExecution>>();
			var executions = await executionRepository.ListAsync(e => e.State == WorkerJobState.Scheduled, cancellationToken);

			foreach (var e in executions)
			{
				e.State = WorkerJobState.Waiting;
			}

			await executionRepository.SaveChangesAsync(cancellationToken);

			var validationRepository = services.GetRequiredService<IRepository<SubmissionValidation>>();
			var validations = await executionRepository.ListAsync(e => e.State == WorkerJobState.Scheduled, cancellationToken);

			foreach (var v in validations)
			{
				v.State = WorkerJobState.Waiting;
			}

			await validationRepository.SaveChangesAsync(cancellationToken);
		}

		/// <inheritdoc />
		protected override async Task ExecuteJob(IServiceProvider scopedProvider,
			CancellationToken cancellationToken)
		{
			var toQueue = maxBrokerItems - await broker.GetUnfinishedTasksCount();
			if (toQueue <= 0)
			{
				return;
			}

			var stats = await broker.GetStats();
			var requests = new List<JobDtoBase>(2 * toQueue);

			var workerService = scopedProvider.GetRequiredService<IWorkerService>();

			var validationRepository =
				scopedProvider.GetRequiredService<ISubmissionValidationRepository>();
			var executionRepository =
				scopedProvider.GetRequiredService<IMatchExecutionRepository>();

			requests.AddRange(await validationRepository.GetRequestsForSchedulingAsync(toQueue,
				WorkerJobState.Waiting, stats.Games, cancellationToken));
			requests.AddRange(await executionRepository.GetRequestsForSchedulingAsync(toQueue,
				WorkerJobState.Waiting, stats.Games, cancellationToken));

			requests.Sort((l, r) => l.Created.CompareTo(r.Created));

			foreach (var job in requests.Take(toQueue))
			{
				switch (job)
				{
					case SubmissionValidationRequestDataDto dto:
						await DispatchValidationRequest(workerService, validationRepository, dto,
							cancellationToken);
						break;

					case MatchExecutionRequestDataDto dto:
						await DispatchMatchExecutionRequest(workerService, executionRepository, dto,
							cancellationToken);
						break;

					default:
						throw new InvalidOperationException("Should never get here");
				}
			}
		}

		public async Task DispatchValidationRequest(IWorkerService workerService,
			IRepository<SubmissionValidation> repository,
			SubmissionValidationRequestDataDto data, CancellationToken cancellationToken)
		{
			var request = new SubmissionValidationRequest
			{
				JobId = data.JobId,
				SubmissionId = data.SubmissionId,
				TournamentId = data.TournamentId,
				ValidationId = data.Id,
				Configuration = data.TournamentConfiguration,
				GameKey = data.GameKey,
				AdditionalFilesUri = workerService.GetAdditionalFilesUrl(data.TournamentId),
				AccessToken = workerService.GenerateWorkerToken(new ClaimsIdentity(new[]
				{
					new Claim(WorkerClaimTypes.SubmissionId, data.SubmissionId.ToString()),
					new Claim(WorkerClaimTypes.ValidationId, data.Id.ToString()),
					new Claim(WorkerClaimTypes.TournamentId, data.TournamentId.ToString())
				}))
			};

			await broker.EnqueueWork(request);
			await repository.UpdateAsync(data.Id,
				new JobStateUpdateDto { State = WorkerJobState.Scheduled },
				cancellationToken);
		}

		public async Task DispatchMatchExecutionRequest(IWorkerService workerService,
			IRepository<MatchExecution> repository, MatchExecutionRequestDataDto data,
			CancellationToken cancellationToken)
		{
			var request = new MatchExecutionRequest
			{
				JobId = data.JobId,
				MatchId = data.MatchId,
				ExecutionId = data.Id,
				TournamentId = data.TournamentId,
				Configuration = data.TournamentConfiguration,
				GameKey = data.GameKey,
				Bots =
					data.SubmissionIds.Select(id => new BotInfo { SubmissionId = id }).ToList(),
				AdditionalFilesUri = workerService.GetAdditionalFilesUrl(data.TournamentId),
				AccessToken = workerService.GenerateWorkerToken(new ClaimsIdentity(data
					.SubmissionIds
					.Select(s => new Claim(WorkerClaimTypes.SubmissionId, s.ToString())).Concat(
						new[]
						{
							new Claim(WorkerClaimTypes.ExecutionId, data.Id.ToString()), new Claim(
								WorkerClaimTypes.TournamentId,
								data.TournamentId.ToString())
						})))
			};

			await broker.EnqueueWork(request);
			await repository.UpdateAsync(data.Id,
				new JobStateUpdateDto { State = WorkerJobState.Scheduled },
				cancellationToken);
		}

		private async Task OnMatchExecuted(IServiceProvider services, MatchExecutionResult result,
			CancellationToken cancellationToken)
		{
			var mediator = services.GetRequiredService<IMediator>();
			var repository = services.GetRequiredService<IRepository<MatchExecution>>();
			var notification = mapper.Map<MatchExecutionFinished>(result);

			// fill notification ids
			var data = await repository.GetAsync(e => e.JobId == result.JobId,
				e => new { e.Id, e.MatchId, e.Match.TournamentId, e.Match.Tournament.GameId },
				cancellationToken);

			notification.Executed = time.Now;
			notification.Exception = result.Exception;
			notification.GameId = data.GameId;
			notification.TournamentId = data.TournamentId;
			notification.ExecutionId = data.Id;
			notification.MatchId = data.MatchId;

			await mediator.Publish(notification, cancellationToken);
		}

		private async Task OnSubmissionValidated(IServiceProvider services,
			SubmissionValidationResult result, CancellationToken cancellationToken)
		{
			var mediator = services.GetRequiredService<IMediator>();
			var repository = services.GetRequiredService<IRepository<SubmissionValidation>>();
			var notification = mapper.Map<SubmissionValidationFinished>(result);

			// fill notification ids
			var data = await repository.GetAsync(e => e.JobId == result.JobId,
				e => new
				{
					e.Id,
					e.SubmissionId,
					e.Submission.TournamentId,
					e.Submission.Tournament.GameId
				}, cancellationToken);

			notification.Executed = time.Now;
			notification.Exception = result.Exception;
			notification.GameId = data.GameId;
			notification.TournamentId = data.TournamentId;
			notification.ValidationId = data.Id;
			notification.SubmissionId = data.SubmissionId;

			await mediator.Publish(notification, cancellationToken);
		}

		private async Task OnTaskExpired(IServiceProvider services, WorkMessageBase msg,
			CancellationToken cancellationToken)
		{
			using var scope = Logger.CreateScopeWithIds(msg);
			Logger.LogWarning(
				$"Execution of job {{{LoggingTags.JobId}}} expired, because there is no worker which could process it",
				msg.JobId);

			switch (msg)
			{
				case SubmissionValidationRequest req:
					Logger.SubmissionValidationExpired(req.JobId);
					await services.GetRequiredService<IRepository<SubmissionValidation>>()
						.UpdateAsync(req.ValidationId,
							new JobStateUpdateDto { State = WorkerJobState.Waiting },
							CancellationToken.None);
					return;

				case MatchExecutionRequest req:
					Logger.MatchExecutionExpired(req.JobId);
					await services.GetRequiredService<IRepository<MatchExecution>>().UpdateAsync(
						req.ExecutionId,
						new JobStateUpdateDto { State = WorkerJobState.Waiting }, cancellationToken);
					return;

				default:
					throw new ArgumentOutOfRangeException(nameof(msg), msg, "Unknown message type");
			}
		}
	}
}