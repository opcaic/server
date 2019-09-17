using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Services;
using OPCAIC.Application.Dtos.Matches;
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
	public class TournamentProcessorTest : ApiServiceTestBase
	{
		/// <inheritdoc />
		public TournamentProcessorTest(ITestOutputHelper output) : base(output)
		{
			UseDatabase();
			Services.Mock<IMediator>(); // no events
			Services.AddMatchGenerators();
			Services.AddMapper();
			Services.AddRepositories();

			LastUpdated = DateTime.MinValue;
			Now = DateTime.Now;
		}

		private DateTime LastUpdated { get; set; }
		private DateTime Now { get; set; }

		private TournamentProcessor.Job Processor
		{
			get
			{
				var services = ServiceProvider;
				Now = Now == LastUpdated ? Now.AddMinutes(1) : Now;
				var lastRun = LastUpdated;
				LastUpdated = Now;

				return new TournamentProcessor.Job(
					services.GetRequiredService<ILogger<TournamentProcessor>>(),
					services.GetRequiredService<IMediator>(),
					services.GetRequiredService<ITournamentRepository>(),
					services.GetRequiredService<IMatchRepository>(),
					services.GetRequiredService<IMatchGenerator>(),
					lastRun,
					Now);
			}
		}

		[Fact]
		public async Task StateTransfer_Finished_Success()
		{
			Faker.Configure<Match>()
				.RuleFor(m => m.Participations,
					f => Faker.Entities<SubmissionParticipation>(2));


			var tournament = NewTrackedEntity<Tournament>();
			tournament.Scope = TournamentScope.Deadline;
			tournament.State = TournamentState.WaitingForFinish;
			tournament.Matches = Faker.Entities<Match>(2);
			await DbContext.SaveChangesAsync();

			await Processor.ExecuteAsync(CancellationToken);

			tournament.State.ShouldBe(TournamentState.Finished);
			tournament.EvaluationFinished.ShouldBe(Now);
		}

		[Fact]
		public async Task StateTransfer_Finished_ExecutorFailed()
		{
			Faker.Configure<Match>()
				.RuleFor(m => m.Participations,
					f => Faker.Entities<SubmissionParticipation>(2))
				.RuleFor(m => m.Executions,
					f => new List<MatchExecution>
					{
						new MatchExecution {ExecutorResult = EntryPointResult.UserError}
					});


			var tournament = NewTrackedEntity<Tournament>();
			tournament.Scope = TournamentScope.Deadline;
			tournament.State = TournamentState.WaitingForFinish;
			tournament.Matches = Faker.Entities<Match>(2);
			await DbContext.SaveChangesAsync();

			await Processor.ExecuteAsync(CancellationToken);

			tournament.State.ShouldBe(TournamentState.WaitingForFinish);
			tournament.EvaluationFinished.ShouldBeNull();
		}

		[Fact]
		public async Task StateTransfer_Running_Deadline()
		{
			var tournament = NewTrackedEntity<Tournament>();
			tournament.State = TournamentState.Published;
			tournament.Deadline = Now.AddMinutes(-1);
			await DbContext.SaveChangesAsync();

			tournament.EvaluationStarted.ShouldBeNull();
			tournament.EvaluationFinished.ShouldBeNull();

			await Processor.ExecuteAsync(CancellationToken);

			tournament.State.ShouldBe(TournamentState.Running);
			tournament.EvaluationStarted.ShouldBe(Now);
			tournament.EvaluationFinished.ShouldBeNull();
		}

		[Fact]
		public async Task StateTransfer_Running_NoDeadline()
		{
			var tournament = NewTrackedEntity<Tournament>();

			tournament.Deadline = null;
			tournament.State = TournamentState.Published;
			await DbContext.SaveChangesAsync();

			tournament.EvaluationStarted.ShouldBeNull();
			tournament.EvaluationFinished.ShouldBeNull();

			await Processor.ExecuteAsync(CancellationToken);

			// no change
			tournament.State.ShouldBe(TournamentState.Published);
			tournament.EvaluationStarted.ShouldBeNull();
			tournament.EvaluationFinished.ShouldBeNull();
		}

		private Tournament SetupTournament(int submissionCount)
		{
			var tournament = NewTrackedEntity<Tournament>();
			tournament.State = TournamentState.Running;
			tournament.Participants = Faker.Entities<Submission>(submissionCount).Select(s => new TournamentParticipation
			{
				Tournament = tournament,
				User = s.Author,
				ActiveSubmission = s,
				Submissions = new List<Submission>
				{
					s
				}
			}).ToList();

			return tournament;
		}

		[Theory]
		[InlineData(TournamentFormat.SinglePlayer, 1, 4, 4)]
		[InlineData(TournamentFormat.Table, 2, 4, 6)]
		public async Task MatchGeneration_Deadline(TournamentFormat format, int arity, int submissionCount, int matchCount)
		{
			var tournament = SetupTournament(submissionCount);
			tournament.Format = format;
			tournament.Scope = TournamentScope.Deadline;
			await DbContext.SaveChangesAsync();

			await Processor.ExecuteAsync(CancellationToken);

			tournament.Matches.ShouldNotBeNull();
			tournament.Matches.Count.ShouldBe(matchCount);
			tournament.Matches.ShouldAllBe(s => s.Participations.Count == arity);
			tournament.State.ShouldBe(TournamentState.WaitingForFinish);
		}

		[Theory]
		[InlineData(TournamentFormat.SingleElimination, 4, 2)]
		[InlineData(TournamentFormat.DoubleElimination, 6, 2)]
		public async Task MatchGeneration_Brackets(TournamentFormat format, int submissions, int matches)
		{
			var tournament = SetupTournament(submissions);
			tournament.Format = format;
			tournament.Scope = TournamentScope.Deadline;
			await DbContext.SaveChangesAsync();

			await Processor.ExecuteAsync(CancellationToken);

			tournament.Matches.ShouldNotBeNull();
			tournament.Matches.Count.ShouldBe(matches);
			tournament.Matches.ShouldAllBe(s => s.Participations.Count == 2);
			tournament.State.ShouldBe(TournamentState.Running);
		}

		[Fact]
		public async Task MatchGeneration_Ongoing()
		{
			var tournament = SetupTournament(5);
			tournament.Format = TournamentFormat.Elo;
			tournament.Scope = TournamentScope.Ongoing;
			tournament.MatchesPerDay = 24;
			tournament.EvaluationStarted = Now - TimeSpan.FromHours(2.1);
			await DbContext.SaveChangesAsync();

			await Processor.ExecuteAsync(CancellationToken);

			tournament.Matches.ShouldNotBeNull();
			tournament.Matches.Count.ShouldBe(2); // 2h have passed
			tournament.Matches.ShouldAllBe(s => s.Participations.Count == 2);
			tournament.State.ShouldBe(TournamentState.Running);
		}

		[Fact]
		public async Task EmptyTournament_Finish()
		{
			var tournament = SetupTournament(0);
			tournament.Deadline = Now.AddMinutes(-1);
			tournament.State = TournamentState.Published;
			tournament.Format = TournamentFormat.Table;
			tournament.Scope = TournamentScope.Deadline;
			await DbContext.SaveChangesAsync();

			await Processor.ExecuteAsync(CancellationToken);

			tournament.State.ShouldBe(TournamentState.Finished);
		}

		[Fact]
		public async Task BracketsGeneration_GeneratesEntireTree()
		{
			var participants = 12;
			var matchRepositoryMock = Services.Mock<IMatchRepository>(MockBehavior.Strict);
			var tournament = SetupTournament(participants);
			tournament.Matches = new List<Match>();
			tournament.Scope = TournamentScope.Deadline;
			tournament.Format = TournamentFormat.SingleElimination;
			await DbContext.SaveChangesAsync();

			await SimulateTournament(matchRepositoryMock, tournament);
		}

		private async Task SimulateTournament(Mock<IMatchRepository> matchRepositoryMock, Tournament tournament)
		{
			// auto execute matches
			matchRepositoryMock.Setup(s
					=> s.CreateMatchesAsync(It.IsNotNull<List<NewMatchDto>>(), CancellationToken))
				.Returns((List<NewMatchDto> matches, CancellationToken token)
					=>
				{
					TestTournamentHelper.ExecuteMatches(tournament, matches, Now);
					return DbContext.SaveChangesAsync(token);
				});

			int matchCount;

			do
			{
				matchCount = tournament.Matches.Count;
				await Processor.ExecuteAsync(CancellationToken);
			} while (matchCount != tournament.Matches.Count);

			tournament.State.ShouldBe(TournamentState.Finished,
				"Tournament match generation got stuck.");
		}

	}
}