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
	public class StopTournamentEvaluationCommandTest : HandlerTest<StopTournamentEvaluationCommand.Handler>
	{
		/// <inheritdoc />
		public StopTournamentEvaluationCommandTest(ITestOutputHelper output) : base(output)
		{
			repository = Services.Mock<IRepository<Tournament>>(MockBehavior.Strict);
		}

		private readonly Mock<IRepository<Tournament>> repository;

		[Fact]
		public async Task Handle_Success()
		{
			repository
				.SetupFind(new Tournament { State = TournamentState.Running, Scope = TournamentScope.Ongoing }, CancellationToken);

			repository
				.SetupUpdate((TournamentStateUpdateDto dto)
					=> dto.State == TournamentState.WaitingForFinish, CancellationToken);

			await Handler.Handle(new StopTournamentEvaluationCommand(), CancellationToken);
		}

		[Fact]
		public async Task Handle_TournamentBadState()
		{
			repository
				.SetupFind(new Tournament { State = TournamentState.Finished, Scope = TournamentScope.Ongoing }, CancellationToken);

			var exception = await Should.ThrowAsync<BadTournamentStateException>(Handler.Handle(new StopTournamentEvaluationCommand(), CancellationToken));
			exception.ActualState.ShouldBe(nameof(TournamentState.Finished));
			exception.ExpectedState.ShouldBe(nameof(TournamentState.Running));
		}


		[Fact]
		public async Task Handle_TournamentBadScope()
		{
			repository
				.SetupFind(new Tournament { State = TournamentState.Running, Scope = TournamentScope.Deadline }, CancellationToken);

			var exception = await Should.ThrowAsync<BadTournamentScopeException>(Handler.Handle(new StopTournamentEvaluationCommand(), CancellationToken));
			exception.ActualScope.ShouldBe(nameof(TournamentScope.Deadline));
			exception.ExpectedScope.ShouldBe(nameof(TournamentScope.Ongoing));
		}
	}
}