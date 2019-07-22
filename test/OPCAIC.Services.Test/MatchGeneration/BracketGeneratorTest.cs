using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.TestUtils;
using Xunit;
using Xunit.Abstractions;
using Match = OPCAIC.Infrastructure.Entities.Match;

namespace OPCAIC.Services.Test.MatchGeneration
{
	public class SingleEliminationGeneratorTest : BracketGeneratorTest
	{
		/// <inheritdoc />
		public SingleEliminationGeneratorTest(ITestOutputHelper output) : base(output)
			=> generator = GetService<SingleEliminationMatchGenerator>();
	}

	public class DoubleEliminationGeneratorTest : BracketGeneratorTest
	{
		/// <inheritdoc />
		public DoubleEliminationGeneratorTest(ITestOutputHelper output) : base(output)
			=> generator = GetService<DoubleEliminationMatchGenerator>();

		[Fact]
		public void GeneratesAdditionalMatchOnTie()
		{
			const int participants = 8;

			var rand = new Random(42);
			var tournament = TournamentDataGenerator.Generate(participants);
			var tree = MatchTreeGenerator.GenerateDoubleElimination(participants);

			// let the second one always win (and therefore the winner of winners bracket will lose in
			// finals
			var executions = Simulate(tournament, i => rand.Next(i), m => 1);

			// all matches are executed (the extra tie breaker is not included in the generated tree)
			Assert.Equal(tree.MatchNodesById.Count + 1, executions.Count);
		}
	}

	public abstract class BracketGeneratorTest : ServiceTestBase
	{
		/// <inheritdoc />
		public BracketGeneratorTest(ITestOutputHelper output) : base(output)
		{
			Services.AddSingleton<IMatchTreeFactory>(new CachedMatchTreeFactory());

			matchRepository = Services.Mock<IMatchRepository>();
			mockMatches = new List<Match>();
			matchRepository.Setup(r => r.AllMatchesFromTournament(It.IsAny<long>()))
				.Returns(mockMatches);
		}

		protected IMatchGenerator generator;
		private readonly Mock<IMatchRepository> matchRepository;
		private readonly List<Match> mockMatches;


		public static TheoryData<int, int> GetSeedsUpTo(int size)
		{
			var data = new TheoryData<int, int>();
			for (var i = 1; i < size; i++)
			{
				data.Add(42 /*seed*/, i /*participants*/);
			}

			return data;
		}

		[Theory]
		[InlineData(42, 42)]
		[MemberData(nameof(GetSeedsUpTo), 3)]
		public void SimpleSimulation(int seed, int participants)
		{
			var rand = new Random(seed);
			var tournament = TournamentDataGenerator.Generate(participants);

			var executions = Simulate(tournament, i => rand.Next(i), m => rand.Next(2));

			// all matches are executed
			Assert.Equal(mockMatches.Count, executions.Count);
		}

		protected List<MatchExecution> Simulate(Tournament tournament, Func<int, int> matchPicker,
			Func<Match, int> resultPicker)
		{
			var toExecute = new List<Match>();
			var executions = new List<MatchExecution>();
			var executionId = 0;

			var (matches, done) = generator.Generate(tournament);
			while (!done)
			{
				toExecute.AddRange(matches);
				mockMatches.AddRange(matches);

				// pick random match
				var matchIdx = matchPicker(toExecute.Count);
				var match = toExecute[matchIdx];
				toExecute.RemoveAt(matchIdx);

				// basic checks
				Assert.Equal(2, match.Participants.Count);
				Assert.NotEqual(match.Participants[0], match.Participants[1]);
				Assert.True(match.Participants.All(p => p != null));

				// execute the match
				var execution = ExecuteMatch(match, executionId++, resultPicker(match));
				match.Executions = new List<MatchExecution> {execution};
				executions.Add(execution);

				// invoke generation of next match
				(matches, done) = generator.Generate(tournament);
			}

			return executions;
		}

		private MatchExecution ExecuteMatch(Match match, int id, int result)
			=> new MatchExecution
			{
				Id = id,
				MatchId = match.Id,
				TournamentId = match.TournamentId,
				Created = DateTime.Today,
				Updated = DateTime.Now,
				Executed = DateTime.Now,
				BotResults = Enumerable.Range(0, 2).Select(i =>
					new SubmissionMatchResult
					{
						SubmissionId = match.Participants[i].Id,
						Submission = match.Participants[i],
						Id = i,
						MatchId = match.Id,
						TournamentId = match.TournamentId,
						ExecutionId = id,
						Score = result == i ? 1 : 0
					}).ToList()
			};

		[Fact]
		public void SimpleStart()
		{
			var tournament = TournamentDataGenerator.Generate(4);

			var (matches, done) = generator.Generate(tournament);

			Assert.False(done); // always done
			Assert.Equal(2, matches.Count()); // bottom matches

			foreach (var match in matches)
			{
				Assert.Equal(MatchState.Waiting, match.MatchState);
				Assert.Equal(2, match.Participants.Count);
				Assert.Contains(string.Join("+", match.Participants.Select(p => p.Author)),
					new[] {"0+3", "1+2"});
			}
		}

		[Fact]
		public void UnevenStart()
		{
			var tournament = TournamentDataGenerator.Generate(6);

			var (matches, done) = generator.Generate(tournament);

			Assert.False(done); // always done
			Assert.Equal(2, matches.Count()); // bottom matches

			foreach (var match in matches)
			{
				Assert.Equal(MatchState.Waiting, match.MatchState);
				Assert.Equal(2, match.Participants.Count);
				Assert.Contains(string.Join("+", match.Participants.Select(p => p.Author)),
					new[] {"3+4", "2+5"});
			}
		}
	}
}