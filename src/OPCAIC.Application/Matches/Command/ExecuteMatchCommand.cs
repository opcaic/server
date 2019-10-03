using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Matches.Command
{
	public class ExecuteMatchCommand : IRequest<long>
	{
		/// <inheritdoc />
		public ExecuteMatchCommand(long matchId)
		{
			MatchId = matchId;
		}

		public long MatchId { get; }

		public class Handler : IRequestHandler<ExecuteMatchCommand, long>
		{
			private readonly ILogger<ExecuteMatchCommand> logger;
			private readonly IRepository<Match> repository;

			public Handler(ILogger<ExecuteMatchCommand> logger,
				IRepository<Match> repository)
			{
				this.repository = repository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<long> Handle(ExecuteMatchCommand request,
				CancellationToken cancellationToken)
			{
				var match = await repository.GetAsync(request.MatchId, cancellationToken);

				// Add the execution
				var execution = new MatchExecution { MatchId = request.MatchId, JobId = Guid.NewGuid() };
				match.LastExecution = execution;
				await repository.SaveChangesAsync(cancellationToken);

				logger.MatchExecutionQueued(execution.Id, request.MatchId, execution.JobId);
				return execution.Id;
			}
		}
	}
}