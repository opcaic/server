using Microsoft.Extensions.DependencyInjection;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.Services;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	public class MatchExecutorTest : WorkerTestBase
	{

		private MatchExecutor MatchExecutor => GetService<MatchExecutor>();

		[Fact]
		public void ExecutesMatchSuccessfully()
		{
			// TODO: more tests once the executor is implemented
			var result = MatchExecutor.Execute(new MatchExecutionRequest());
			Assert.Equal(Status.Ok, result.Status);
		}

		/// <inheritdoc />
		public MatchExecutorTest(ITestOutputHelper output) : base(output)
		{
			Services.AddTransient<MatchExecutor>();
			Services.Mock<IDownloadService>();
		}
	}
}
