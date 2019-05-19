using OPCAIC.Messaging.Messages;

namespace OPCAIC.Worker.Services
{
	class MatchExecutor : IJobExecutor<MatchExecutionRequest, MatchExecutionResult>
	{
		/// <inheritdoc />
		public MatchExecutionResult Execute(MatchExecutionRequest request)
		{
			return new MatchExecutionResult()
			{
				Id = request.Id,
				Status = Status.Ok
			};
		}
	}
}