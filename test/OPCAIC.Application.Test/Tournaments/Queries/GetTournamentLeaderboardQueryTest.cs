using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces.MatchGeneration;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Application.Tournaments.Queries;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using Shouldly;
using Xunit;
using Xunit.Abstractions;
using static OPCAIC.Application.Test.TestMapper;
using Match = OPCAIC.Domain.Entities.Match;

namespace OPCAIC.Application.Test.Tournaments.Queries
{
	public class GetTournamentLeaderboardQueryTest
		: HandlerTest<GetTournamentLeaderboardQuery.Handler>
	{
		/// <inheritdoc />
		public GetTournamentLeaderboardQueryTest(ITestOutputHelper output) : base(output)
		{
			Services.AddMemoryCache();
			Services.AddMatchGenerators();
			repository = Services.Mock<IRepository<Tournament>>(MockBehavior.Strict);
			matchRepository = Services.Mock<IRepository<Match>>(MockBehavior.Strict);
			ConfigureEntities();
		}

		private readonly Mock<IRepository<Tournament>> repository;
		private readonly Mock<IRepository<Match>> matchRepository;

		private const int tournamentId = 23;

		private readonly GetTournamentLeaderboardQuery query =
			new GetTournamentLeaderboardQuery(tournamentId);

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

		private void SetupTournament(TournamentScope scope, TournamentFormat format,
			int submissionCount, int seed = 100)
		{
			var tournament = Faker.Configure<Tournament>()
				.RuleFor(t => t.Game, Faker.Entity<Game>).Generate();
			tournament.Id = tournamentId;
			tournament.Scope = scope;
			tournament.Format = format;
			tournament.RankingStrategy = TournamentRankingStrategy.Maximum;
			tournament.EvaluationFinished = DateTime.Now;
			if (scope == TournamentScope.Ongoing)
			{
				tournament.Deadline = null;
			}

			tournament.Participants = Faker.Entities<Submission>(submissionCount).Select(s
				=> new TournamentParticipation
				{
					Tournament = tournament,
					User = s.Author,
					Submissions = new List<Submission> {s},
					ActiveSubmission = s,
					ActiveSubmissionId = s.Id
				}).ToList();

			var generator = GetService<IMatchGenerator>();
			TestTournamentHelper.SimulateTournament(null, tournament, generator, seed);

			repository.Setup(r
				=> r.FindAsync(
					It.IsAny<IProjectingSpecification<Tournament,
						GetTournamentLeaderboardQuery.Handler.Data>>(),
					CancellationToken)).ReturnsAsync(
				(IProjectingSpecification<Tournament, GetTournamentLeaderboardQuery.Handler.Data> spec,
					CancellationToken token) => spec.Projection.Compile().Invoke(tournament));

			matchRepository.SetupProjectList(()
				=> Mapper.Map<List<GetTournamentLeaderboardQuery.Handler.MatchData>>(tournament
					.Matches), CancellationToken);
		}

		public static TheoryData<int> GetRange(int from, int to)
		{
			var theoryData = new TheoryData<int>();
			for (int i = from; i <= to; i++)
			{
				theoryData.Add(i /*data*/);
			}

			return theoryData;
		}

		[Theory]
		[MemberData(nameof(GetRange), 4, 8)]
		public async Task Generate_DoubleElimination(int submissionCount)
		{
			SetupTournament(TournamentScope.Deadline, TournamentFormat.DoubleElimination,
				submissionCount);
			var leaderboards = await Handler.Handle(query, CancellationToken);
			var tree = leaderboards.ShouldBeOfType<DoubleEliminationLeaderboardDto>();
			leaderboards.Participations.Count.ShouldBe(submissionCount);
			leaderboards.Finished.ShouldBe(true);

			for (int i = 0; i < 4; i++)
			{
				leaderboards.Participations[i].Place.ShouldBe(i + 1);
			}
		}

		[Fact]
		public async Task Generate_Elo()
		{
			SetupTournament(TournamentScope.Ongoing, TournamentFormat.Elo, 10);
			var leaderboards = await Handler.Handle(query, CancellationToken);
			leaderboards.Participations.Count.ShouldBe(10);
			leaderboards.Finished.ShouldBe(true);
			leaderboards.Participations[0].Place.ShouldBe(1);
		}

		[Fact]
		public async Task Generate_SingleElimination()
		{
			SetupTournament(TournamentScope.Deadline, TournamentFormat.SingleElimination, 4);
			var leaderboards = await Handler.Handle(query, CancellationToken);
			var tree = leaderboards.ShouldBeOfType<SingleEliminationLeaderboardDto>();
			leaderboards.Participations.Count.ShouldBe(4);
			leaderboards.Finished.ShouldBe(true);

			for (int i = 0; i < 4; i++)
			{
				leaderboards.Participations[i].Place.ShouldBe(i + 1);
			}
		}

		[Fact]
		public async Task Generate_SinglePlayer()
		{
			SetupTournament(TournamentScope.Deadline, TournamentFormat.SinglePlayer, 10);
			var leaderboards = await Handler.Handle(query, CancellationToken);
			leaderboards.Participations.Count.ShouldBe(10);
			leaderboards.Finished.ShouldBe(true);
			leaderboards.Participations[0].Place.ShouldBe(1);
		}


		[Fact]
		public async Task Generate_Table()
		{
			SetupTournament(TournamentScope.Deadline, TournamentFormat.Table, 10);
			var leaderboards = await Handler.Handle(query, CancellationToken);
			leaderboards.Participations.Count.ShouldBe(10);
			leaderboards.Finished.ShouldBe(true);
			leaderboards.Participations[0].Place.ShouldBe(1);
		}
	}
}