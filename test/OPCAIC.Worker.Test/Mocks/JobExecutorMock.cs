using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.Services;

namespace OPCAIC.Worker.Test.Mocks
{
	internal class JobExecutorMock<TRequest, TResponse> : IJobExecutor<TRequest, TResponse>
		where TResponse : ReplyMessageBase, new() where TRequest : WorkMessageBase

	{
		/// <inheritdoc />
		public TResponse Execute(TRequest request)
			=> new TResponse
			{
				Id = request.Id,
				Status = Status.Ok
			};
	}
}
