using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.Services;
using Xunit;

namespace OPCAIC.Worker.Test
{
	public class MatchExecutorTest
	{
		public MatchExecutorTest() => MatchExecutor = new MatchExecutor();

		private MatchExecutor MatchExecutor { get; }

		[Fact]
		public void ExecutesMatchSuccessfully()
		{
			// TODO: more tests once the executor is implemented
			var result = MatchExecutor.Execute(new MatchExecutionRequest());
			Assert.Equal(Status.Ok, result.Status);
		}
	}
}
