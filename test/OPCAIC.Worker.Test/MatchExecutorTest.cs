using Moq;
using OPCAIC.Messaging.Messages;
using OPCAIC.TestUtils;
using OPCAIC.Worker.Services;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	public class MatchExecutorTest : ServiceTestBase
	{
		/// <inheritdoc />
		public MatchExecutorTest(ITestOutputHelper output) : base(output)
		{
			Services.Mock<IExecutionServices>()
				.Setup(s => s.GetWorkingDirectory(It.IsAny<WorkMessageBase>()))
				.Returns(NewDirectory);
		}

		private MatchExecutor MatchExecutor => GetService<MatchExecutor>();

		[Fact]
		public void ExecutesMatchSuccessfully()
		{
			// TODO: more tests once the executor is implemented
			var result = MatchExecutor.ExecuteAsync(new MatchExecutionRequest()).Result;
			Assert.Equal(JobStatus.Ok, result.JobStatus);
		}
	}
}
