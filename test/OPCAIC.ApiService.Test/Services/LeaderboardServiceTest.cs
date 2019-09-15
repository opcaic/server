using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Models.Leaderboards;
using OPCAIC.ApiService.Services;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.MatchGeneration;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using Match = OPCAIC.Domain.Entities.Match;

namespace OPCAIC.ApiService.Test.Services
{
	public class LeaderboardServiceTest : ApiServiceTestBase
	{
		private const int tournamentId = 23;
		private readonly Mock<IMatchRepository> matchRepository;
		private readonly Mock<ISubmissionRepository> submissionRepository;
		private readonly Mock<ITournamentRepository> tournamentRepository;

		/// <inheritdoc />
		public LeaderboardServiceTest(ITestOutputHelper output) : base(output)
		{
			// setup services
			matchRepository = Services.Mock<IMatchRepository>(MockBehavior.Strict);
			tournamentRepository = Services.Mock<ITournamentRepository>(MockBehavior.Strict);
			submissionRepository = Services.Mock<ISubmissionRepository>(MockBehavior.Strict);
			UseDatabase();
			Services.AddMapper();
			Services.AddMatchGenerators();
			Services.AddSingleton<LeaderboardService>();

			//ConfigureEntities();
		}

		private LeaderboardService LeaderboardService => GetService<LeaderboardService>();

		private void SetupTournament(TournamentScope scope, TournamentFormat format,
			int submissionCount, int seed = 100)
		{
			var tournament = Faker.Entity<Tournament>();
			tournament.Id = tournamentId;
			tournament.Scope = scope;
			tournament.Format = format;
			if (scope == TournamentScope.Ongoing)
			{
				tournament.Deadline = null;
			}

			tournament.Submissions = Faker.Entities<Submission>(submissionCount);

			DbContext.Add(tournament);
			DbContext.SaveChanges();
			var generator = GetService<IMatchGenerator>();
			TestTournamentHelper.SimulateTournament(tournament, generator, seed);

			tournamentRepository.Setup(r => r.ExistsByIdAsync(tournamentId, CancellationToken))
				.ReturnsAsync(true);
			tournamentRepository.Setup(r => r.FindByIdAsync(tournamentId, CancellationToken))
				.ReturnsAsync(() => Mapper.Map<TournamentDetailDto>(tournament));

			matchRepository.Setup(r
					=> r.AllMatchesFromTournamentAsync(tournamentId, CancellationToken))
				.ReturnsAsync(() => Mapper.Map<List<MatchDetailDto>>(tournament.Matches));

			submissionRepository.Setup(r
					=> r.AllSubmissionsFromTournament(tournamentId, CancellationToken))
				.ReturnsAsync(() => Mapper.Map<List<SubmissionDetailDto>>(tournament.Submissions));

			DbContext.SaveChanges();
		}

		private void ConfigureEntities()
		{
			// configure id too because we do not want to use EF to keep tests fast
			Faker.Configure<Match>()
				.RuleFor(m => m.Id, f => f.IndexFaker);

			Faker.Configure<MatchExecution>()
				.RuleFor(m => m.Id, f => f.IndexFaker);

			Faker.Configure<SubmissionMatchResult>()
				.RuleFor(m => m.Id, f => f.IndexFaker);

			Faker.Configure<SubmissionMatchResult>()
				.RuleFor(m => m.Id, f => f.IndexFaker);

			Faker.Configure<Submission>()
				.RuleFor(m => m.Id, f => f.IndexFaker);

			Faker.Configure<User>()
				.RuleFor(m => m.Id, f => f.IndexFaker);

			Faker.Configure<Tournament>()
				.RuleFor(m => m.Id, f => f.IndexFaker);
		}

		[Fact]
		public async Task Generate_SinglePlayer()
		{
			SetupTournament(TournamentScope.Deadline, TournamentFormat.SinglePlayer, 10, 100);
			var leaderboards =
				await LeaderboardService.GetTournamentLeaderboard(tournamentId, CancellationToken);
			leaderboards.Participations.Count.ShouldBe(10);
			leaderboards.Finished.ShouldBe(true);
			leaderboards.Participations[0].Place.ShouldBe(1);
		}


		[Fact]
		public async Task Generate_Table()
		{
			SetupTournament(TournamentScope.Deadline, TournamentFormat.Table, 10, 100);
			var leaderboards =
				await LeaderboardService.GetTournamentLeaderboard(tournamentId, CancellationToken);
			leaderboards.Participations.Count.ShouldBe(10);
			leaderboards.Finished.ShouldBe(true);
			leaderboards.Participations[0].Place.ShouldBe(1);
		}

		[Fact]
		public async Task Generate_Elo()
		{
			SetupTournament(TournamentScope.Ongoing, TournamentFormat.Elo, 10, 100);
			var leaderboards =
				await LeaderboardService.GetTournamentLeaderboard(tournamentId, CancellationToken);
			leaderboards.Participations.Count.ShouldBe(10);
			leaderboards.Finished.ShouldBe(true);
			leaderboards.Participations[0].Place.ShouldBe(1);
		}

		[Fact]
		public async Task Generate_SingleElimination()
		{
			SetupTournament(TournamentScope.Deadline, TournamentFormat.SingleElimination, 4, 100);
			var leaderboards =
				await LeaderboardService.GetTournamentLeaderboard(tournamentId, CancellationToken);
			var tree = (SingleEliminationTreeLeaderboardModel)leaderboards;
			leaderboards.Participations.Count.ShouldBe(4);
			leaderboards.Finished.ShouldBe(true);
			leaderboards.Participations[0].Place.ShouldBe(1);
			tree.Final.FirstPlayerOriginMatch.ShouldNotBeNull();
			tree.Final.SecondPlayerOriginMatch.ShouldNotBeNull();
		}

		[Fact]
		public async Task Generate_DoubleElimination()
		{
			SetupTournament(TournamentScope.Deadline, TournamentFormat.DoubleElimination, 4, 100);
			var leaderboards =
				await LeaderboardService.GetTournamentLeaderboard(tournamentId, CancellationToken);
			var tree = (DoubleEliminationTreeLeaderboardModel)leaderboards;
			leaderboards.Participations.Count.ShouldBe(4);
			leaderboards.Finished.ShouldBe(true);
			leaderboards.Participations[0].Place.ShouldBe(1);
			tree.Final.FirstPlayerOriginMatch.MatchId.ShouldBe(tree.WinnersBracketFinal.MatchId);
			tree.Final.SecondPlayerOriginMatch.MatchId.ShouldBe(tree.LosersBracketFinal.MatchId);
			tree.WinnersBracketFinal.FirstPlayerOriginMatch.ShouldNotBeNull();
			tree.LosersBracketFinal.FirstPlayerOriginMatch.ShouldNotBeNull();
			tree.WinnersBracketFinal.SecondPlayerOriginMatch.ShouldNotBeNull();
			tree.LosersBracketFinal.SecondPlayerOriginMatch.ShouldNotBeNull();
		}
	}
}