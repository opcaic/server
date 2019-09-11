using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Services;
using OPCAIC.Services.Extensions;
using Xunit;
using Xunit.Abstractions;
using Match = OPCAIC.Infrastructure.Entities.Match;

namespace OPCAIC.ApiService.Test.Services
{
	public class LeaderboardServiceTest : ApiServiceTestBase
	{
		private readonly Mock<IMatchRepository> matchRepository;
		private readonly Mock<ITournamentRepository> tournamentRepository;
		private const int tournamentId = 23;

		/// <inheritdoc />
		public LeaderboardServiceTest(ITestOutputHelper output) : base(output)
		{
			// setup services
			matchRepository = Services.Mock<IMatchRepository>(MockBehavior.Strict);
			tournamentRepository = Services.Mock<ITournamentRepository>(MockBehavior.Strict);
			UseDatabase();
			Services.AddMapper();
			Services.AddMatchGenerators();
			Services.AddSingleton<LeaderboardService>();

			//ConfigureEntities();
		}

		private LeaderboardService LeaderboardService => GetService<LeaderboardService>();

		private void SetupTournament(TournamentScope scope, TournamentFormat format, int submissionCount, int seed = 100)
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

			DbContext.SaveChanges();
		}

		private void ConfigureEntities()
		{
			// configure id too because we do not want to use EF to keep tests fast
			Faker.Configure<Infrastructure.Entities.Match>()
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

		[Theory]
		[InlineData(TournamentScope.Deadline, TournamentFormat.SinglePlayer, 10)]
		[InlineData(TournamentScope.Deadline, TournamentFormat.DoubleElimination, 10)]
		[InlineData(TournamentScope.Deadline, TournamentFormat.SingleElimination, 10)]
		[InlineData(TournamentScope.Deadline, TournamentFormat.Table, 10)]
		[InlineData(TournamentScope.Ongoing, TournamentFormat.Elo, 10)]
		public Task SimpleGenerationTest(TournamentScope scope, TournamentFormat format, int submissions)
		{
			// TODO: separate tests into individual facts and test more thoroughly
			SetupTournament(scope, format, submissions);
			return LeaderboardService.GetTournamentLeaderboard(tournamentId, CancellationToken);
		}
	}
}