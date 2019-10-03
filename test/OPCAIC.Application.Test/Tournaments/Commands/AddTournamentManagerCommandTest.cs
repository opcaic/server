using Moq;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Command;
using OPCAIC.Domain.Entities;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.Tournaments.Commands
{
	public class AddTournamentManagerCommandTest : HandlerTest<AddTournamentManagerCommand.Handler>
	{
		/// <inheritdoc />
		public AddTournamentManagerCommandTest(ITestOutputHelper output) : base(output)
		{
			repository = Services.Mock<IRepository<Tournament>>(MockBehavior.Strict);
			userRepository = Services.Mock<IRepository<User>>(MockBehavior.Strict);
			managerRepository = Services.Mock<IRepository<TournamentManager>>(MockBehavior.Strict);
		}

		private readonly Mock<IRepository<Tournament>> repository;
		private readonly Mock<IRepository<TournamentManager>> managerRepository;
		private readonly Mock<IRepository<User>> userRepository;

		[Fact]
		public async Task Handle_Success()
		{
			repository.SetupFind(new Tournament(), CancellationToken);
			userRepository.SetupFind(new User(), CancellationToken);

			managerRepository
				.Setup(r => r.ExistsAsync(It.IsAny<ISpecification<TournamentManager>>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(false);

			managerRepository
				.Setup(r => r.Add(It.IsAny<TournamentManager>()));

			await Handler.Handle(new AddTournamentManagerCommand(), CancellationToken.None);
		}

		[Fact]
		public async Task Handle_Failure()
		{
			repository.SetupFind(new Tournament(), CancellationToken);

			managerRepository
				.Setup(r => r.ExistsAsync(It.IsAny<ISpecification<TournamentManager>>(),
					It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);

			userRepository
				.SetupFind(new User() { Email = "email" }, CancellationToken);


			var exception = await Should.ThrowAsync<UserIsAlreadyManagerOfTournamentException>(
				Handler.Handle(new AddTournamentManagerCommand() { TournamentId = 1 }, CancellationToken.None));
			exception.UserEmail.ShouldBe("email");
			exception.TournamentId.ShouldBe(1);
		}
	}
}