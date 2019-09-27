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
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.TestUtils;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.Handlers.Tournaments
{
	public class StopTournamentEvaluationCommandHandlerTest : TournamentCommandHandlerTestBase
	{
		/// <inheritdoc />
		public StopTournamentEvaluationCommandHandlerTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Handle_NonExistingIDs()
		{
			repository
				.Setup(r => r.FindByIdAsync(It.IsAny<long>(), CancellationToken.None))
				.ReturnsAsync(default(TournamentDetailDto));

			var exception = await Should.ThrowAsync<NotFoundException>(
				GetService<StopTournamentEvaluationCommand.Handler>()
					.Handle(new StopTournamentEvaluationCommand(), CancellationToken.None));
			exception.Resource.ShouldBe(nameof(Tournament));
		}

		[Fact]
		public async Task Handle_Success()
		{
			repository
				.Setup(r => r.FindByIdAsync(It.IsAny<long>(), CancellationToken.None))
				.ReturnsAsync(new TournamentDetailDto { State = TournamentState.Running, Scope = TournamentScope.Ongoing});

			repository
				.Setup(r => r.UpdateAsync(It.IsAny<ISpecification<Tournament>>(),
					It.IsAny<TournamentStateUpdateDto>(), It.IsAny<CancellationToken>()));

			await GetService<StopTournamentEvaluationCommand.Handler>()
				.Handle(new StopTournamentEvaluationCommand(), CancellationToken.None);
		}

		[Fact]
		public async Task Handle_TournamentBadState()
		{
			repository
				.Setup(r => r.FindByIdAsync(It.IsAny<long>(), CancellationToken.None))
				.ReturnsAsync(new TournamentDetailDto { State = TournamentState.Published });

			var exception = await Should.ThrowAsync<BadTournamentStateException>(
				GetService<StopTournamentEvaluationCommand.Handler>()
					.Handle(new StopTournamentEvaluationCommand(), CancellationToken.None));
			exception.ActualState.ShouldBe(nameof(TournamentState.Published));
			exception.ExpectedState.ShouldBe(nameof(TournamentState.Running));
		}


		[Fact]
		public async Task Handle_TournamentBadScope()
		{
			repository
				.Setup(r => r.FindByIdAsync(It.IsAny<long>(), CancellationToken.None))
				.ReturnsAsync(new TournamentDetailDto { Scope = TournamentScope.Deadline, State = TournamentState.Running});

			var exception = await Should.ThrowAsync<BadTournamentScopeException>(
				GetService<StopTournamentEvaluationCommand.Handler>()
					.Handle(new StopTournamentEvaluationCommand(), CancellationToken.None));
			exception.ActualScope.ShouldBe(nameof(TournamentScope.Deadline));
			exception.ExpectedScope.ShouldBe(nameof(TournamentScope.Ongoing));
		}
	}
}