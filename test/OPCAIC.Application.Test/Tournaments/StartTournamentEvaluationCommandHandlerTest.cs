using System.Threading;
using System.Threading.Tasks;
using Moq;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Test.Tournaments;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.TestUtils;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.Handlers.Tournaments
{
	public class StartTournamentEvaluationCommandHandlerTest : TournamentCommandHandlerTestBase
	{
		/// <inheritdoc />
		public StartTournamentEvaluationCommandHandlerTest(ITestOutputHelper output) : base(output)
		{
			Services.Mock<ITimeService>();
		}

		[Fact]
		public async Task Handle_NonExistingIDs()
		{
			repository
				.Setup(r => r.FindByIdAsync(It.IsAny<long>(), CancellationToken.None))
				.ReturnsAsync(default(TournamentDetailDto));

			var exception = await Should.ThrowAsync<NotFoundException>(
				GetService<StartTournamentEvaluationCommand.Handler>()
					.Handle(new StartTournamentEvaluationCommand(), CancellationToken.None));
			exception.Resource.ShouldBe(nameof(Tournament));
		}

		[Fact]
		public async Task Handle_Success()
		{
			repository
				.Setup(r => r.FindByIdAsync(It.IsAny<long>(), CancellationToken.None))
				.ReturnsAsync(new TournamentDetailDto {State = TournamentState.Published});

			repository
				.Setup(r => r.UpdateAsync(It.IsAny<ISpecification<Tournament>>(),
					It.IsAny<TournamentStateUpdateDto>(), It.IsAny<CancellationToken>()));

			await GetService<StartTournamentEvaluationCommand.Handler>()
				.Handle(new StartTournamentEvaluationCommand(), CancellationToken.None);
		}

		[Fact]
		public async Task Handle_TournamentBadState()
		{
			repository
				.Setup(r => r.FindByIdAsync(It.IsAny<long>(), CancellationToken.None))
				.ReturnsAsync(new TournamentDetailDto {State = TournamentState.Running});

			var exception = await Should.ThrowAsync<BadTournamentStateException>(
				GetService<StartTournamentEvaluationCommand.Handler>()
					.Handle(new StartTournamentEvaluationCommand(), CancellationToken.None));
			exception.ActualState.ShouldBe(nameof(TournamentState.Running));
			exception.ExpectedState.ShouldBe(nameof(TournamentState.Published));
		}
	}
}