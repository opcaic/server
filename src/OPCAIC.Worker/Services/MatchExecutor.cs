using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Worker.Services
{
	internal class MatchExecutor : JobExecutorBase<MatchExecutionRequest, MatchExecutionResult>
	{
		/// <inheritdoc />
		public MatchExecutor(ILogger<MatchExecutor> logger, IExecutionServices services) : base(logger, services)
		{
		}

		/// <inheritdoc />
		protected override Task InternalExecute()
		{
			Result.Status = Status.Ok;
			return Task.CompletedTask;
		}
	}
}
