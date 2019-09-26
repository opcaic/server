using Microsoft.Extensions.DependencyInjection;
using Moq;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Services;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Enums;
using OPCAIC.Messaging.Messages;
using System.Threading.Tasks;
using OPCAIC.Application.Tournaments.Models;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Services
{
	public class SubmissionScoreServiceTest : ApiServiceTestBase
	{
		/// <inheritdoc />
		public SubmissionScoreServiceTest(ITestOutputHelper output) : base(output)
		{
			submissionRepository = Services.Mock<ISubmissionRepository>(MockBehavior.Strict);
			tournamentRepository = Services.Mock<ITournamentRepository>(MockBehavior.Strict);
			Services.AddMapper();
			Services.AddSingleton<SubmissionScoreService>();
		}

		private readonly Mock<ISubmissionRepository> submissionRepository;
		private readonly Mock<ITournamentRepository> tournamentRepository;

		private readonly SubmissionDetailDto eloSubmission =
			new SubmissionDetailDto
			{
				Id = 1,
				Score = 1200,
				Tournament = new TournamentReferenceDto { Id = 2 }
			};

		private readonly SubmissionDetailDto tableSubmission
			= new SubmissionDetailDto
			{
				Id = 2,
				Score = 100,
				Tournament = new TournamentReferenceDto { Id = 1 }
			};

		private readonly SubmissionDetailDto singlePlayerSubmission =
			new SubmissionDetailDto
			{
				Id = 3,
				Score = 0,
				Tournament = new TournamentReferenceDto { Id = 3 }
			};

		private readonly SubmissionDetailDto eloSubmission2 =
			new SubmissionDetailDto { Id = 4, Score = 1200 };

		private readonly SubmissionDetailDto tableSubmission2
			= new SubmissionDetailDto { Id = 5, Score = 100 };

		private readonly TournamentDetailDto tableTournament =
			new TournamentDetailDto { Format = TournamentFormat.Table, Id = 1 };

		private readonly TournamentDetailDto eloTournament =
			new TournamentDetailDto { Format = TournamentFormat.Elo, Id = 2 };

		private readonly TournamentDetailDto singlePlayerTournament =
			new TournamentDetailDto
			{
				Format = TournamentFormat.SinglePlayer,
				RankingStrategy = TournamentRankingStrategy.Maximum,
				Id = 3
			};

		[Fact]
		public async Task UpdateEloScore_Success()
		{
			submissionRepository
				.Setup(r => r.FindByIdAsync(eloSubmission.Id, CancellationToken))
				.ReturnsAsync(eloSubmission);
			submissionRepository
				.Setup(r => r.FindByIdAsync(eloSubmission2.Id, CancellationToken))
				.ReturnsAsync(eloSubmission2);
			tournamentRepository
				.Setup(r => r.FindByIdAsync(eloTournament.Id, CancellationToken))
				.ReturnsAsync(eloTournament);
			submissionRepository
				.Setup(r => r.UpdateAsync(eloSubmission.Id,
					It.IsAny<UpdateSubmissionScoreDto>(), CancellationToken))
				.ReturnsAsync(true);
			submissionRepository
				.Setup(r => r.UpdateAsync(eloSubmission2.Id,
					It.IsAny<UpdateSubmissionScoreDto>(), CancellationToken))
				.ReturnsAsync(true);
			await GetService<SubmissionScoreService>().UpdateSubmissionsScore(
				new MatchExecutionResult
				{
					BotResults = new[]
					{
						new BotResult {SubmissionId = eloSubmission.Id},
						new BotResult {SubmissionId = eloSubmission2.Id}
					}
				}, CancellationToken);
		}

		[Fact]
		public async Task UpdateSinglePlayerScore_Success()
		{
			submissionRepository
				.Setup(r => r.FindByIdAsync(singlePlayerSubmission.Id, CancellationToken))
				.ReturnsAsync(singlePlayerSubmission);
			tournamentRepository
				.Setup(r => r.FindByIdAsync(singlePlayerTournament.Id, CancellationToken))
				.ReturnsAsync(singlePlayerTournament);
			submissionRepository
				.Setup(r => r.UpdateAsync(singlePlayerSubmission.Id,
					It.IsAny<UpdateSubmissionScoreDto>(), CancellationToken))
				.ReturnsAsync(true);
			await GetService<SubmissionScoreService>().UpdateSubmissionsScore(
				new MatchExecutionResult
				{
					BotResults = new[]
					{
						new BotResult {SubmissionId = singlePlayerSubmission.Id}
					}
				}, CancellationToken);
		}

		[Fact]
		public async Task UpdateTableScore_Success()
		{
			submissionRepository
				.Setup(r => r.FindByIdAsync(tableSubmission.Id, CancellationToken))
				.ReturnsAsync(tableSubmission);
			submissionRepository
				.Setup(r => r.FindByIdAsync(tableSubmission2.Id, CancellationToken))
				.ReturnsAsync(tableSubmission2);
			tournamentRepository
				.Setup(r => r.FindByIdAsync(tableTournament.Id, CancellationToken))
				.ReturnsAsync(tableTournament);
			submissionRepository
				.Setup(r => r.UpdateAsync(tableSubmission.Id,
					It.IsAny<UpdateSubmissionScoreDto>(), CancellationToken))
				.ReturnsAsync(true);
			submissionRepository
				.Setup(r => r.UpdateAsync(tableSubmission2.Id,
					It.IsAny<UpdateSubmissionScoreDto>(), CancellationToken))
				.ReturnsAsync(true);
			await GetService<SubmissionScoreService>().UpdateSubmissionsScore(
				new MatchExecutionResult
				{
					BotResults = new[]
					{
						new BotResult {SubmissionId = tableSubmission.Id},
						new BotResult {SubmissionId = tableSubmission2.Id}
					}
				}, CancellationToken);
		}
	}
}