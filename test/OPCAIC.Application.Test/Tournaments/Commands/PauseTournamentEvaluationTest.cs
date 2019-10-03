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
	public class PauseTournamentEvaluationTest : HandlerTest<PauseTournamentEvaluationCommand.Handler>
	{
		/// <inheritdoc />
		public PauseTournamentEvaluationTest(ITestOutputHelper output) : base(output)
		{
			repository = Services.Mock<IRepository<Tournament>>(MockBehavior.Strict);
		}

		private readonly Mock<IRepository<Tournament>> repository;

		[Fact]
		public async Task Handle_Success()
		{
			repository
				.SetupFind(new Tournament { State = TournamentState.Running }, CancellationToken);

			repository
				.SetupUpdate((TournamentStateUpdateDto dto)
					=> dto.State == TournamentState.Paused, CancellationToken);

			await Handler.Handle(new PauseTournamentEvaluationCommand(), CancellationToken);
		}

		[Fact]
		public async Task Handle_TournamentBadState()
		{
			repository
				.SetupFind(new Tournament { State = TournamentState.Finished }, CancellationToken);

			var exception = await Should.ThrowAsync<BadTournamentStateException>(Handler.Handle(new PauseTournamentEvaluationCommand(), CancellationToken));
			exception.ActualState.ShouldBe(nameof(TournamentState.Finished));
			exception.ExpectedState.ShouldBe(nameof(TournamentState.Running));
		}
	}
}