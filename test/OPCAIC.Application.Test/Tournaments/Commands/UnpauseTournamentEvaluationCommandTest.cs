using Moq;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Shouldly;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.Tournaments.Commands
{
	public class UnpauseTournamentEvaluationCommandTest : HandlerTest<UnpauseTournamentEvaluationCommand.Handler>
	{
		/// <inheritdoc />
		public UnpauseTournamentEvaluationCommandTest(ITestOutputHelper output) :
			base(output)
		{
			repository = Services.Mock<IRepository<Tournament>>(MockBehavior.Strict);
		}

		private readonly Mock<IRepository<Tournament>> repository;

		[Fact]
		public async Task Handle_Success()
		{
			repository
				.SetupFind(new Tournament() { State = TournamentState.Paused }, CancellationToken);

			repository
				.SetupUpdate((TournamentStateUpdateDto dto)
					=> dto.State == TournamentState.Running, CancellationToken);

			await Handler.Handle(new UnpauseTournamentEvaluationCommand(), CancellationToken);
		}

		[Fact]
		public async Task Handle_TournamentBadState()
		{
			repository
				.SetupFind(new Tournament { State = TournamentState.Finished }, CancellationToken);

			var exception = await Should.ThrowAsync<BadTournamentStateException>(Handler.Handle(new UnpauseTournamentEvaluationCommand(), CancellationToken));
			exception.ActualState.ShouldBe(nameof(TournamentState.Finished));
			exception.ExpectedState.ShouldBe(nameof(TournamentState.Paused));
		}
	}
}