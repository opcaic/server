using System;
using System.Collections.Generic;
using System.Linq;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.MatchGeneration;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.Persistence;
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

		public static void SimulateTournament(DataContext context, Tournament tournament, IMatchGenerator generator, int seed = 100)
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
						ExecuteMatches(context, tournament, matches, DateTime.Now, seed + tournament.Matches.Count);
					} while (!done);
					break;
				case TournamentFormat.Table:
				case TournamentFormat.SinglePlayer:
					matches = generator.GenerateDeadline(
						TestMapper.Mapper.Map<TournamentDeadlineGenerationDto>(tournament));
					ExecuteMatches(context, tournament, matches, DateTime.Now, seed);
					break;
				case TournamentFormat.Elo:
					matches = generator.GenerateOngoing(
						TestMapper.Mapper.Map<TournamentOngoingGenerationDto>(tournament),
						tournament.Participants.Count * 3);
					ExecuteMatches(context, tournament, matches, DateTime.Now, seed);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			tournament.State = TournamentState.Finished;
		}

		public static void ExecuteMatches(DataContext context, Tournament tournament,
			List<NewMatchDto> matches,
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
							Submission = tournament.Participants.Select(p => p.ActiveSubmission).Single(sub => sub.Id == s),
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
								Submission = tournament.Participants.Select(p => p.ActiveSubmission).Single(sub => sub.Id == s),
								CompilerResult = EntryPointResult.Success,
								Crashed = false,
								Score = (score = (score + 1 % 2))
							}).ToList()
						}
					},
				});
			}

			context.SaveChanges();
			foreach (var match in tournament.Matches)
			{
				match.LastExecution = match.Executions.Single();
			}
		}
	}
}