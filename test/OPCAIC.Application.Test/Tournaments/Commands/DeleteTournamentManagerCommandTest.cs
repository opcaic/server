using Moq;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Domain.Entities;
using Shouldly;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.Tournaments.Commands
{
	public class DeleteTournamentManagerCommandTest : HandlerTest<DeleteTournamentManagerCommand.Handler>
	{
		/// <inheritdoc />
		public DeleteTournamentManagerCommandTest(ITestOutputHelper output) : base(output)
		{
			repository = Services.Mock<IRepository<Tournament>>(MockBehavior.Strict);
			managerRepository = Services.Mock<IRepository<TournamentManager>>(MockBehavior.Strict);
		}

		private readonly Mock<IRepository<Tournament>> repository;
		private readonly Mock<IRepository<TournamentManager>> managerRepository;

		[Fact]
		public async Task Handle_Success()
		{
			repository
				.Setup(r => r.ExistsAsync(It.IsAny<ISpecification<Tournament>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);

			managerRepository
				.SetupDelete(It.IsAny<CancellationToken>());

			await Handler.Handle(new DeleteTournamentManagerCommand(), CancellationToken.None);
		}

		[Fact]
		public async Task Handle_Failure()
		{
			repository
				.Setup(r => r.ExistsAsync(It.IsAny<ISpecification<Tournament>>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(true);

			managerRepository
				.SetupDelete(It.IsAny<CancellationToken>(), false);


			await Should.ThrowAsync<UserIsNotManagerOfTournamentException>(Handler.Handle(new DeleteTournamentManagerCommand(), CancellationToken.None));
		}
	}
}
