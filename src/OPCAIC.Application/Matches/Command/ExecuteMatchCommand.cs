using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;

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
			private readonly IMatchExecutionRepository repository;

			public Handler(ILogger<ExecuteMatchCommand> logger,
				IMatchExecutionRepository repository)
			{
				this.repository = repository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<long> Handle(ExecuteMatchCommand request,
				CancellationToken cancellationToken)
			{
				var execution =
					new NewMatchExecutionDto {MatchId = request.MatchId, JobId = Guid.NewGuid()};
				var id = await repository.CreateAsync(execution, cancellationToken);
				logger.MatchExecutionQueued(id, request.MatchId, execution.JobId);
				return id;
			}
		}
	}
}