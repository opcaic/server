using System;
using System.Collections.Generic;
using System.Linq;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Services;
using Shouldly;

namespace OPCAIC.ApiService.Test.Services
{
	internal static class TestTournamentHelper
	{
		public static void ShouldBeInValidState(this Tournament tournament)
		{
			tournament.Scope.ShouldNotBe(TournamentScope.Unknown);
			tournament.Format.ShouldNotBe(TournamentFormat.Unknown);
			tournament.Availability.ShouldNotBe(TournamentAvailability.Unknown);

			if (tournament.Scope == TournamentScope.Ongoing)
			{
				// others do not really make sense
				tournament.Format.ShouldBe(TournamentFormat.Elo);

				tournament.Deadline.ShouldBeNull("Ongoing tournaments cannot have deadline");
			}
		}

		public static void SimulateTournament(Tournament tournament, IMatchGenerator generator, int seed = 100)
		{
			tournament.ShouldBeInValidState();

			List<NewMatchDto> matches;
			switch (tournament.Format)
			{
				case TournamentFormat.SingleElimination:
				case TournamentFormat.DoubleElimination:
					bool done;
					do
					{
						(matches, done) =
							generator.GenerateBrackets(
								TestMapper.Mapper.Map<TournamentBracketsGenerationDto>(tournament));
						ExecuteMatches(tournament, matches, DateTime.Now, seed + tournament.Matches.Count);
					} while (!done);
					break;
				case TournamentFormat.Table:
				case TournamentFormat.SinglePlayer:
					matches = generator.GenerateDeadline(
						TestMapper.Mapper.Map<TournamentDeadlineGenerationDto>(tournament));
					ExecuteMatches(tournament, matches, DateTime.Now, seed);
					break;
				case TournamentFormat.Elo:
					matches = generator.GenerateOngoing(
						TestMapper.Mapper.Map<TournamentOngoingGenerationDto>(tournament), tournament.Submissions.Count * 3);
					ExecuteMatches(tournament, matches, DateTime.Now, seed);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			tournament.State = TournamentState.Finished;
		}

		public static void ExecuteMatches(Tournament tournament, List<NewMatchDto> matches,
			DateTime now, int seed = 100)
		{
			matches.ShouldAllBe(m => m.Submissions.Count == matches[0].Submissions.Count);
			var rand = new Random(100);

			foreach (var matchDto in matches)
			{
				var order = 0;
				var score = rand.Next(2);

				tournament.Matches.Add(new Match
				{
					Participations = matchDto.Submissions
						.Select(s => new SubmissionParticipation
						{
							Submission = tournament.Submissions.Single(sub => sub.Id == s),
							SubmissionId = s,
							Order = order++
						}).ToList(),
					Index = matchDto.Index,
					Executions = new List<MatchExecution>
					{
						new MatchExecution
						{
							AdditionalData = "{}",
							ExecutorResult = EntryPointResult.Success,
							JobId = Guid.NewGuid(),
							State = WorkerJobState.Finished,
							Executed = now,
							BotResults = matchDto.Submissions.Select(s => new SubmissionMatchResult
							{
								AdditionalData = "{}",
								SubmissionId = s,
								Submission = tournament.Submissions.Single(sub => sub.Id == s),
								CompilerResult = EntryPointResult.Success,
								Crashed = false,
								Score = (score = (score + 1 % 2))
							}).ToList()
						}
					}
				});
			}
		}
	}
}