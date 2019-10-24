using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Emails.Templates;
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
		private readonly Mock<IRepository<TournamentInvitation>> repository;
		private readonly Mock<IRepository<TournamentParticipation>> participationsRepository;
		private readonly Mock<IRepository<User>> userRepository;
		/// <inheritdoc />
		public InvitePlayersCommandTest(ITestOutputHelper output) : base(output)
		{
			emailService = Services.Mock<IEmailService>(MockBehavior.Strict);
			urlGenerator = Services.Mock<IFrontendUrlGenerator>(MockBehavior.Strict);
			tournamentRepository = Services.Mock<ITournamentRepository>(MockBehavior.Strict);
			repository = Services.Mock<IRepository<TournamentInvitation>>(MockBehavior.Strict);
			participationsRepository =
				Services.Mock<IRepository<TournamentParticipation>>(MockBehavior.Strict);
			userRepository = Services.Mock<IRepository<User>>(MockBehavior.Strict);
		}

		[Fact]
		public async Task Success_NonexistingUser()
		{
			long tournamentId = 1;
			var newmail = "newmail@mail.cz";
			var oldmail = "mail@mail.cz";
			var emails = new[] {newmail, oldmail};

			tournamentRepository
				.Setup(r => r.FindAsync(It.IsAny<IProjectingSpecification<Tournament, string>>(), CancellationToken))
				.ReturnsAsync("Tournament");

			repository
				.Setup(r => r.ListAsync(It.IsAny<
						IProjectingSpecification<TournamentInvitation, string>>(),
					CancellationToken))
				.ReturnsAsync(new List<string> { oldmail });

			// expect only new mail to be created
			repository
				.Setup(r => r.Add(It.Is<TournamentInvitation>(i => i.TournamentId == tournamentId && i.UserId == null && i.Email == newmail)));

			userRepository.Setup(r => r.FindAsync(It.IsAny<ProjectingSpecification<User, long?>>(),
					CancellationToken))
				.ReturnsAsync((long?)null);

			urlGenerator
				.Setup(r => r.TournamentPageLink(tournamentId))
				.Returns("url");

			emailService
				.Setup(r => r.EnqueueEmailAsync(It.IsAny<EmailData>(), newmail,
					CancellationToken))
				.Returns(Task.CompletedTask);

			participationsRepository.Setup(s => s.SaveChangesAsync(CancellationToken))
				.Returns(Task.CompletedTask);
			repository.Setup(s => s.SaveChangesAsync(CancellationToken))
				.Returns(Task.CompletedTask);

			await Handler.Handle(new InvitePlayersCommand
			{
				Emails = emails,
				TournamentId = tournamentId,
			}, CancellationToken);
		}

		[Fact]
		public async Task Success_ExistingUser()
		{
			long tournamentId = 1;
			long userId = 1;
			var email = "newmail@mail.cz";
			var emails = new[] { email };

			tournamentRepository
				.Setup(r => r.FindAsync(It.IsAny<IProjectingSpecification<Tournament, string>>(), CancellationToken))
				.ReturnsAsync("Tournament");

			repository
				.Setup(r => r.ListAsync(It.IsAny<
						IProjectingSpecification<TournamentInvitation, string>>(),
					CancellationToken))
				.ReturnsAsync(new List<string> {});

			repository
				.Setup(r => r.Add(It.Is<TournamentInvitation>(i => i.TournamentId == tournamentId && i.UserId == userId && i.Email == email)));

			userRepository.Setup(r => r.FindAsync(It.IsAny<ProjectingSpecification<User, long?>>(),
					CancellationToken))
				.ReturnsAsync(userId);

			participationsRepository.Setup(r => r.Add(It.Is<TournamentParticipation>(e => e.TournamentId == tournamentId && e.UserId == userId)));

			urlGenerator
				.Setup(r => r.TournamentPageLink(tournamentId))
				.Returns("url");

			emailService
				.Setup(r => r.EnqueueEmailAsync(It.IsAny<EmailData>(), email,
					CancellationToken))
				.Returns(Task.CompletedTask);

			participationsRepository.Setup(s => s.SaveChangesAsync(CancellationToken))
				.Returns(Task.CompletedTask);
			repository.Setup(s => s.SaveChangesAsync(CancellationToken))
				.Returns(Task.CompletedTask);

			await Handler.Handle(new InvitePlayersCommand
			{
				Emails = emails,
				TournamentId = tournamentId,
			}, CancellationToken);
		}
	}
}