using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.MatchGeneration;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Application.MatchExecutions.Models;
using OPCAIC.Application.Services;
using OPCAIC.Application.Services.MatchGeneration;
using OPCAIC.Domain.Enums;
using OPCAIC.TestUtils;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Application.Test.MatchGeneration
{
	public class SingleEliminationGeneratorTest : BracketGeneratorTest
	{
		/// <inheritdoc />
		public SingleEliminationGeneratorTest(ITestOutputHelper output) : base(output)
		{
			generator = GetService<SingleEliminationMatchGenerator>();
		}

		/// <inheritdoc />
		protected override TournamentFormat GetTournamentFormat()
		{
			return TournamentFormat.SingleElimination;
		}
	}

	public class DoubleEliminationGeneratorTest : BracketGeneratorTest
	{
		/// <inheritdoc />
		public DoubleEliminationGeneratorTest(ITestOutputHelper output) : base(output)
		{
			generator = GetService<DoubleEliminationMatchGenerator>();
		}

		/// <inheritdoc />
		protected override TournamentFormat GetTournamentFormat()
		{
			return TournamentFormat.DoubleElimination;
		}

		[Fact]
		public void GeneratesAdditionalMatchOnTie()
		{
			const int participants = 8;

			var rand = new Random(42);
			var extTournament = GenerateTournament(participants);
			var tree = MatchTreeGenerator.GenerateDoubleElimination(participants);

			// let the second one always win (and therefore the winner of winners bracket will lose in
			// finals
			var executions = Simulate(extTournament, i => rand.Next(i), m => 1);

			// all matches are executed
			executions.Count.ShouldBe(tree.MatchNodesById.Count);
		}
	}

	public abstract class BracketGeneratorTest : ServiceTestBase
	{
		/// <inheritdoc />
		public BracketGeneratorTest(ITestOutputHelper output) : base(output)
		{
			Services.AddSingleton<IMatchTreeFactory>(new CachedMatchTreeFactory(new MemoryCache(new MemoryCacheOptions())));

			Services.Mock<IMatchRepository>();
		}

		protected IBracketsMatchGenerator generator;

		public static TheoryData<int, int> GetSeedsUpTo(int size)
		{
			var data = new TheoryData<int, int>();
			for (var i = 1; i < size; i++)
			{
				data.Add(42 /*seed*/, i /*participants*/);
			}

			return data;
		}

		protected TournamentBracketsGenerationDto GenerateTournament(int participants)
		{
			var tournament =
				TournamentDataGenerator.Generate(participants, GetTournamentFormat());
			var extTournament = new TournamentBracketsGenerationDto
			{
				Id = tournament.Id,
				ActiveSubmissionIds = tournament.ActiveSubmissionIds,
				Matches = new List<MatchDetailDto>(),
				Format = tournament.Format,
				RankingStrategy = TournamentRankingStrategy.Maximum
			};
			return extTournament;
		}

		protected abstract TournamentFormat GetTournamentFormat();

		[Theory]
		[InlineData(42, 42)]
		[InlineData(42, 13)]
		[MemberData(nameof(GetSeedsUpTo), 5)]
		public void SimpleSimulation(int seed, int participants)
		{
			var rand = new Random(seed);
			var tournament = GenerateTournament(participants);

			int totalMatches;
			long specialIndex;
			MatchTreeBase tree;

			if (GetTournamentFormat() == TournamentFormat.DoubleElimination)
			{
				var myTree = MatchTreeGenerator.GenerateDoubleElimination(participants);
				tree = myTree;
				totalMatches = tree.MatchNodesById.Count;
				specialIndex = myTree.Final?.MatchIndex ?? -1;
			}
			else
			{
				tree = MatchTreeGenerator.GenerateSingleElimination(participants, true);
				totalMatches = tree.MatchNodesById.Count;
				specialIndex = -1; // none
			}

			var executions = Simulate(tournament, i => rand.Next(i),
				m => m.Index == specialIndex
				? 1 // make sure the additional match executes
				: rand.Next(2));

			// all matches are executed
			executions.Count.ShouldBe(tournament.Matches.Count);
			executions.Count.ShouldBe(totalMatches);
		}

		protected List<MatchExecutionDetailDto> Simulate(TournamentBracketsGenerationDto tournament,
			Func<int, int> matchPicker,
			Func<MatchDetailDto, int> resultPicker)
		{
			var toExecute = new List<MatchDetailDto>();
			var executions = new List<MatchExecutionDetailDto>();
			var tournamentRef =
				new MatchDetailDto.TournamentDto
				{
					Id = tournament.Id,
					Name = "Mock Tournament",
					Format = tournament.Format,
				};

			var (matches, done) = generator.Generate(tournament);
			while (!done || matches.Count + toExecute.Count > 0)
			{
				var newMatches = matches.Select(m => new MatchDetailDto
				{
					Tournament = tournamentRef,
					Submissions =
						m.Submissions.ConvertAll(i => new SubmissionReferenceDto {Id = i}),
					Index = m.Index,
					LastExecution = new MatchExecutionDetailDto {Created = DateTime.Now}
				}).ToList();

				toExecute.AddRange(newMatches);
				tournament.Matches.AddRange(newMatches); // to be fetched next time

				// pick some match
				var matchIdx = matchPicker(toExecute.Count);
				var match = toExecute[matchIdx];
				toExecute.RemoveAt(matchIdx);

				// basic checks
				match.Submissions.Count.ShouldBe(2);
				match.Submissions[0].ShouldNotBe(match.Submissions[1]);

				// execute the match
				ExecuteMatch(match.LastExecution, match.Submissions, resultPicker(match));
				executions.Add(match.LastExecution);

				// invoke generation of next match
				(matches, done) = generator.Generate(tournament);
			}

			return executions;
		}

		private void ExecuteMatch(MatchExecutionDetailDto execution,
			IList<SubmissionReferenceDto> submissions, int result)
		{
			execution.Executed = DateTime.Now;
			execution.AdditionalData = new JObject();
			execution.ExecutorResult = EntryPointResult.Success;
			execution.BotResults = Enumerable.Range(0, 2).Select(i =>
				new MatchExecutionDetailDto.SubmissionResultDetailDto
				{
					Submission = new SubmissionReferenceDto {Id = submissions[i].Id},
					Score = result == i ? 1 : 0,
					AdditionalData = new JObject(),
					CompilerResult = EntryPointResult.Success
				}).ToList();
		}

		[Fact]
		public void SimpleStart()
		{
			var tournament = GenerateTournament(4);

			var (matches, done) = generator.Generate(tournament);

			done.ShouldBe(false);
			matches.Count.ShouldBe(2); // bottom matches

			foreach (var match in matches)
			{
				match.Submissions.Count.ShouldBe(2);
				new[] {"0+3", "1+2"}.ShouldContain(string.Join("+", match.Submissions));
			}
		}

		[Fact]
		public void UnevenStart()
		{
			var tournament = GenerateTournament(6);

			var (matches, done) = generator.Generate(tournament);

			done.ShouldBe(false);
			matches.Count.ShouldBe(2); // bottom matches

			foreach (var match in matches)
			{
				match.Submissions.Count.ShouldBe(2);
				new[] {"3+4", "2+5"}.ShouldContain(string.Join("+", match.Submissions));
			}
		}
	}
}