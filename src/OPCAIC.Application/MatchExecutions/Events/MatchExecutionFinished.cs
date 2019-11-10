using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure.Events;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Specifications;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Application.MatchExecutions.Events
{
	public class MatchExecutionFinished : MatchExecutionEvent
	{
		public JobStatus JobStatus { get; set; }
		public BotResult[] BotResults { get; set; }
		public SubTaskResult ExecutorResult { get; set; }
		public DateTime Executed { get; set; }
		public Exception Exception { get; set; }
		public Dictionary<string, object> AdditionalData { get; set; } =
			new Dictionary<string, object>();

		public class Handler : INotificationHandler<MatchExecutionFinished>
		{
			private readonly ILogger<MatchExecutionFinished> logger;
			private readonly IMapper mapper;
			private readonly IRepository<MatchExecution> repository;
			private readonly ISubmissionScoreService scoreService;

			public Handler(ILogger<MatchExecutionFinished> logger, IMapper mapper, IRepository<MatchExecution> repository, ISubmissionScoreService scoreService)
			{
				this.logger = logger;
				this.mapper = mapper;
				this.repository = repository;
				this.scoreService = scoreService;
			}

			/// <inheritdoc />
			public async Task Handle(MatchExecutionFinished notification, CancellationToken cancellationToken)
			{
				var execution = await repository.GetAsync(notification.ExecutionId, new[] {nameof(MatchExecution.BotResults)}, cancellationToken);

				execution.BotResults.Clear();
				mapper.Map(notification, execution);

				for (var i = 0; i < execution.BotResults.Count; i++)
				{
					execution.BotResults[i].Order = i;
				}

				if (notification.ExecutorResult == SubTaskResult.Ok)
				{
					await scoreService.UpdateSubmissionsScore(notification, new CancellationToken());
				}

				await repository.SaveChangesAsync(cancellationToken);
				logger.MatchExecutionUpdated(notification.JobId, notification);
			}
		}
	}
}