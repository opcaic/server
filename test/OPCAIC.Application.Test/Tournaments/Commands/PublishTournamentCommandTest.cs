using System.Threading.Tasks;
using MediatR;
using Moq;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Shouldly;
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
			mediator = Services.Mock<IMediator>(MockBehavior.Strict);
		}

		private readonly Mock<IRepository<Tournament>> repository;
		private readonly Mock<IMediator> mediator;

		[Theory]
		[InlineData(TournamentScope.Deadline)]
		[InlineData(TournamentScope.Ongoing)]
		public async Task Handle_Success(TournamentScope scope)
		{
			if (scope == TournamentScope.Ongoing)
			{
				mediator.Setup(s => s.Send(It.IsAny<StartTournamentEvaluationCommand>(), CancellationToken)).ReturnsAsync(Unit.Value);
			}

			repository
				.SetupFind(new Tournament {State = TournamentState.Created, Scope = scope},
					CancellationToken);

			repository
				.SetupUpdate((TournamentStateUpdateDto dto)
					=> dto.State == TournamentState.Published, CancellationToken);

			await Handler.Handle(new PublishTournamentCommand(), CancellationToken);
		}

		[Fact]
		public async Task Handle_TournamentBadState()
		{
			repository
				.SetupFind(new Tournament {State = TournamentState.Finished}, CancellationToken);

			var exception =
				await Should.ThrowAsync<BadTournamentStateException>(
					Handler.Handle(new PublishTournamentCommand(), CancellationToken));
			exception.ActualState.ShouldBe(nameof(TournamentState.Finished));
			exception.ExpectedState.ShouldBe(nameof(TournamentState.Created));
		}
	}
}