using System.Threading;
using System.Threading.Tasks;
using Moq;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.Tournaments.Commands
{
	public class PublishTournamentCommandTest : TournamentHandlerTestBase
	{
		/// <inheritdoc />
		public PublishTournamentCommandTest(ITestOutputHelper output) : base(output)
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
				GetService<PublishTournamentCommand.Handler>()
					.Handle(new PublishTournamentCommand(), CancellationToken.None));
			exception.Resource.ShouldBe(nameof(Tournament));
		}

		[Fact]
		public async Task Handle_Success()
		{
			repository
				.Setup(r => r.FindByIdAsync(It.IsAny<long>(), CancellationToken.None))
				.ReturnsAsync(new TournamentDetailDto { State = TournamentState.Created });

			repository
				.Setup(r => r.UpdateAsync(It.IsAny<ISpecification<Tournament>>(),
					It.IsAny<TournamentStateUpdateDto>(), It.IsAny<CancellationToken>()));

			await GetService<PublishTournamentCommand.Handler>()
				.Handle(new PublishTournamentCommand(), CancellationToken.None);
		}

		[Fact]
		public async Task Handle_TournamentBadState()
		{
			repository
				.Setup(r => r.FindByIdAsync(It.IsAny<long>(), CancellationToken.None))
				.ReturnsAsync(new TournamentDetailDto { State = TournamentState.Published });

			var exception = await Should.ThrowAsync<BadTournamentStateException>(
				GetService<PublishTournamentCommand.Handler>()
					.Handle(new PublishTournamentCommand(), CancellationToken.None));
			exception.ActualState.ShouldBe(nameof(TournamentState.Published));
			exception.ExpectedState.ShouldBe(nameof(TournamentState.Created));
		}
	}
}