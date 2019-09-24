using System.Threading.Tasks;
using Moq;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.TournamentInvitations.Commands;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.TournamentInvitations.Commands
{
	public class DeleteInvitationCommandTest : HandlerTest<DeleteInvitationCommand.Handler>
	{
		/// <inheritdoc />
		public DeleteInvitationCommandTest(ITestOutputHelper output) : base(output)
		{
			repository = Services.Mock<ITournamentInvitationRepository>(MockBehavior.Strict);
		}

		private readonly Mock<ITournamentInvitationRepository> repository;

		[Fact]
		public Task Success()
		{
			repository
				.Setup(r => r.DeleteAsync(0, "", CancellationToken))
				.ReturnsAsync(false);

			return Handler.Handle(new DeleteInvitationCommand(0, ""), CancellationToken);
		}
	}
}