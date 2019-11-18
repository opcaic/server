using System.Threading.Tasks;
using Moq;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.TournamentInvitations.Commands;
using OPCAIC.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.TournamentInvitations.Commands
{
	public class DeleteInvitationCommandTest : HandlerTest<DeleteInvitationCommand.Handler>
	{
		/// <inheritdoc />
		public DeleteInvitationCommandTest(ITestOutputHelper output) : base(output)
		{
			invitationRepository = Services.Mock<IRepository<TournamentInvitation>>(MockBehavior.Strict);
			participationsRepository = Services.Mock<IRepository<TournamentParticipation>>(MockBehavior.Strict);

			invitationRepository.SetupFind(Invitation, CancellationToken);
			invitationRepository.SetupDelete(Invitation);
		}

		private readonly Mock<IRepository<TournamentInvitation>> invitationRepository;
		private readonly Mock<IRepository<TournamentParticipation>> participationsRepository;

		private TournamentInvitation Invitation = new TournamentInvitation
		{
			UserId = null,
			TournamentId = 1,
			Email = "a@a.com"
		};

		[Fact]
		public Task Success_NoUser()
		{
			invitationRepository.SetupDelete(CancellationToken);
			return Handler.Handle(new DeleteInvitationCommand(Invitation.TournamentId, Invitation.Email), CancellationToken);
		}

		[Fact]
		public Task Success_ExistingUser()
		{
			Invitation.UserId = 1;

			invitationRepository.SetupDelete(CancellationToken);
			participationsRepository.SetupDelete(CancellationToken);
			return Handler.Handle(new DeleteInvitationCommand(Invitation.TournamentId, Invitation.Email), CancellationToken);
		}
	}
}