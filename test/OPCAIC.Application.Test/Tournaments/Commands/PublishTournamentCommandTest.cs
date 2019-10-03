using Moq;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Shouldly;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.Tournaments.Commands
{
	public class PublishTournamentCommandTest : HandlerTest<PublishTournamentCommand.Handler>
	{
		/// <inheritdoc />
		public PublishTournamentCommandTest(ITestOutputHelper output) : base(output)
		{
			Services.Mock<ITimeService>();
			repository = Services.Mock<IRepository<Tournament>>(MockBehavior.Strict);
		}

		private readonly Mock<IRepository<Tournament>> repository;

		[Fact]
		public async Task Handle_Success()
		{
			repository
				.SetupFind(new Tournament { State = TournamentState.Created }, CancellationToken);

			repository
				.SetupUpdate((TournamentStateUpdateDto dto)
					=> dto.State == TournamentState.Published, CancellationToken);

			await Handler.Handle(new PublishTournamentCommand(), CancellationToken);
		}

		[Fact]
		public async Task Handle_TournamentBadState()
		{
			repository
				.SetupFind(new Tournament { State = TournamentState.Finished }, CancellationToken);

			var exception =
				await Should.ThrowAsync<BadTournamentStateException>(
					Handler.Handle(new PublishTournamentCommand(), CancellationToken));
			exception.ActualState.ShouldBe(nameof(TournamentState.Finished));
			exception.ExpectedState.ShouldBe(nameof(TournamentState.Created));
		}
	}
}