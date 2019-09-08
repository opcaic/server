using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Services;
using OPCAIC.Services.Extensions;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Services
{
	public class TournamentProcessorTest : ApiServiceTestBase
	{
		/// <inheritdoc />
		public TournamentProcessorTest(ITestOutputHelper output) : base(output)
		{
			UseDatabase();
			Services.AddMatchGenerators();
			Services.AddMapper();
			Services.AddRepositories();

			LastUpdated = DateTime.MinValue;
			Now = DateTime.Now;
		}

		private DateTime LastUpdated { get; }
		private DateTime Now { get; }

		private TournamentProcessor.Job Processor
		{
			get
			{
				var services = ServiceProvider;

				return new TournamentProcessor.Job(
					services.GetRequiredService<ILogger<TournamentProcessor>>(),
					services.GetRequiredService<ITournamentRepository>(),
					services.GetRequiredService<IMatchRepository>(),
					services.GetRequiredService<IMatchGenerator>(),
					LastUpdated,
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
			tournament.Submissions = Faker.Entities<Submission>(submissionCount);
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
	}
}