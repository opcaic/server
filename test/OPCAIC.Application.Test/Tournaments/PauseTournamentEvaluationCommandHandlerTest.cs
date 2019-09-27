using System.Threading;
using System.Threading.Tasks;
using Moq;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Test.Tournaments;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.Handlers.Tournaments
{
	public class PauseTournamentEvaluationCommandHandlerTest : TournamentCommandHandlerTestBase
	{
		/// <inheritdoc />
		public PauseTournamentEvaluationCommandHandlerTest(ITestOutputHelper output) : base(output)
		{
		}

		[Fact]
		public async Task Handle_NonExistingIDs()
		{
			repository
				.Setup(r => r.FindByIdAsync(It.IsAny<long>(), CancellationToken.None))
				.ReturnsAsync(default(TournamentDetailDto));

			var exception = await Should.ThrowAsync<NotFoundException>(
				GetService<PauseTournamentEvaluationCommand.Handler>()
					.Handle(new PauseTournamentEvaluationCommand(), CancellationToken.None));
			exception.Resource.ShouldBe(nameof(Tournament));
		}

		[Fact]
		public async Task Handle_Success()
		{
			repository
				.Setup(r => r.FindByIdAsync(It.IsAny<long>(), CancellationToken.None))
				.ReturnsAsync(new TournamentDetailDto {State = TournamentState.Running});

			repository
				.Setup(r => r.UpdateAsync(It.IsAny<ISpecification<Tournament>>(),
					It.IsAny<TournamentStateUpdateDto>(), It.IsAny<CancellationToken>()));

			await GetService<PauseTournamentEvaluationCommand.Handler>()
				.Handle(new PauseTournamentEvaluationCommand(), CancellationToken.None);
		}

		[Fact]
		public async Task Handle_TournamentBadState()
		{
			repository
				.Setup(r => r.FindByIdAsync(It.IsAny<long>(), CancellationToken.None))
				.ReturnsAsync(new TournamentDetailDto {State = TournamentState.Finished});

			var exception = await Should.ThrowAsync<BadTournamentStateException>(
				GetService<PauseTournamentEvaluationCommand.Handler>()
					.Handle(new PauseTournamentEvaluationCommand(), CancellationToken.None));
			exception.ActualState.ShouldBe(nameof(TournamentState.Finished));
			exception.ExpectedState.ShouldBe(nameof(TournamentState.Running));
		}
	}
}