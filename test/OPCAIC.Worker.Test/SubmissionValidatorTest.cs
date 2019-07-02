using OPCAIC.Messaging.Messages;
using OPCAIC.TestUtils;
using OPCAIC.Worker.Services;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	public class SubmissionValidatorTest : ServiceTestBase
	{
		/// <inheritdoc />
		public SubmissionValidatorTest(ITestOutputHelper output) : base(output)
			=> Services.Mock<IExecutionServices>();

		private SubmissionValidator SubmissionValidator => GetService<SubmissionValidator>();

		[Fact(Skip = "Not Implemented")]
		public void ExecutesMatchSuccessfully()
		{
			// TODO: more tests once the executor is implemented
			var result = SubmissionValidator.Execute(new SubmissionValidationRequest());
			Assert.Equal(Status.Ok, result.Status);
		}
	}
}