﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.SubmissionValidations.Events;
using OPCAIC.Broker;
using OPCAIC.Common;
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

		/// <inheritdoc />
		protected override async Task ExecuteJob(IServiceProvider scopedProvider,
			CancellationToken cancellationToken)
		{
			// TODO: filter the queues by available games
			var toQueue = maxBrokerItems - await broker.GetUnfinishedTasksCount();
			if (toQueue <= 0)
			{
				return;
			}

			var stats = await broker.GetStats();
			var requests = new List<JobDtoBase>(2 * toQueue);

			var validationService =
				scopedProvider.GetRequiredService<ISubmissionValidationService>();
			var validationRepository =
				scopedProvider.GetRequiredService<ISubmissionValidationRepository>();
			var executionService = scopedProvider.GetRequiredService<IMatchExecutionService>();
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
						await broker.EnqueueWork(validationService.CreateRequest(dto));
						await validationRepository.UpdateJobStateAsync(dto.JobId,
							new JobStateUpdateDto {State = WorkerJobState.Scheduled},
							cancellationToken);
						break;

					case MatchExecutionRequestDataDto dto:
						await broker.EnqueueWork(executionService.CreateRequest(dto));
						await executionRepository.UpdateJobStateAsync(dto.JobId,
							new JobStateUpdateDto {State = WorkerJobState.Scheduled},
							cancellationToken);
						break;

					default:
						throw new InvalidOperationException("Should never get here");
				}
			}
		}

		private async Task OnMatchExecuted(IServiceProvider services, MatchExecutionResult result,
			CancellationToken cancellationToken)
		{
			using (Logger.BeginScope((LoggingTags.JobId, result.JobId)))
			{
				var executionService = services.GetRequiredService<IMatchExecutionService>();
				await executionService.UpdateFromMessage(result);
			}
		}

		private async Task OnSubmissionValidated(IServiceProvider services,
			SubmissionValidationResult result, CancellationToken cancellationToken)
		{
			using (Logger.BeginScope((LoggingTags.JobId, result.JobId)))
			{
				var dto = mapper.Map<SubmissionValidationFinished>(result);
				dto.Executed = time.Now;
				await services.GetRequiredService<IMediator>().Publish(dto, cancellationToken);
			}
		}

		private async Task OnTaskExpired(IServiceProvider services, WorkMessageBase msg,
			CancellationToken cancellationToken)
		{
			Logger.LogWarning(
				$"Execution of job {{{LoggingTags.JobId}}} expired, because there is no worker which could process it",
				msg.JobId);
			switch (msg)
			{
				case SubmissionValidationRequest req:
					var validationService =
						services.GetRequiredService<ISubmissionValidationService>();
					await validationService.OnValidationRequestExpired(req.JobId);
					return;

				case MatchExecutionRequest req:
					var executionService = services.GetRequiredService<IMatchExecutionService>();
					await executionService.OnExecutionRequestExpired(req.JobId);
					return;

				default:
					throw new ArgumentOutOfRangeException(nameof(msg), msg, "Unknown message type");
			}
		}
	}
}