using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OPCAIC.Application.Dtos.EmailTemplates;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.TournamentInvitations.Commands;
using OPCAIC.Application.TournamentInvitations.Models;
using OPCAIC.Domain.Entities;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.TournamentInvitations.Commands
{
	public class InvitePlayersCommandTest : HandlerTest<InvitePlayersCommand.Handler>
	{
		private readonly Mock<IEmailService> emailService;
		private readonly Mock<IFrontendUrlGenerator> urlGenerator;
		private readonly Mock<ITournamentRepository> tournamentRepository;
		private readonly Mock<ITournamentInvitationRepository> repository;
		/// <inheritdoc />
		public InvitePlayersCommandTest(ITestOutputHelper output) : base(output)
		{
			emailService = Services.Mock<IEmailService>(MockBehavior.Strict);
			urlGenerator = Services.Mock<IFrontendUrlGenerator>(MockBehavior.Strict);
			tournamentRepository = Services.Mock<ITournamentRepository>(MockBehavior.Strict);
			repository = Services.Mock<ITournamentInvitationRepository>(MockBehavior.Strict);
		}

		[Fact]
		public async Task Success()
		{
			long tournamentId = 1;
			var newmail = "newmail@mail.cz";
			var oldmail = "mail@mail.cz";
			var emails = new[] {newmail, oldmail};

			tournamentRepository
				.Setup(r => r.FindAsync(It.IsAny<IProjectingSpecification<Tournament, string>>(), CancellationToken.None))
				.ReturnsAsync("Tournament");

			repository
				.Setup(r => r.ListAsync(It.IsAny<
						IProjectingSpecification<TournamentInvitation, string>>(),
					CancellationToken.None))
				.ReturnsAsync(new List<string> { oldmail });

			// expect only new mail to be created
			repository
				.Setup(r => r.CreateAsync(tournamentId, new List<string>{newmail},
					CancellationToken.None))
				.ReturnsAsync(true);

			urlGenerator
				.Setup(r => r.TournamentPageLink(tournamentId))
				.Returns("url");

			emailService
				.Setup(r => r.EnqueueEmailAsync(It.IsAny<EmailDtoBase>(), newmail,
					CancellationToken.None))
				.Returns(Task.CompletedTask);

			await Handler.Handle(new InvitePlayersCommand
			{
				Emails = emails,
				TournamentId = tournamentId,
			}, CancellationToken.None);
		}
	}
}