using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces.MatchGeneration;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Tournaments.Events;
using OPCAIC.Domain.Enums;

namespace OPCAIC.ApiService.Services
{
	public class TournamentProcessor : HostedJob
	{
		private DateTime lastUpdated = DateTime.MinValue;

		/// <inheritdoc />
		public TournamentProcessor(IServiceScopeFactory scopeFactory,
			ILogger<TournamentProcessor> logger)
			: base(scopeFactory, logger, TimeSpan.FromSeconds(5))
		{
		}

		/// <inheritdoc />
		protected override Task ExecuteJob(IServiceProvider scopedProvider,
			CancellationToken cancellationToken)
		{
			var now = DateTime.Now;
			var lastRun = lastUpdated;
			lastUpdated = now;
			return new Job(
				Logger,
				scopedProvider.GetRequiredService<IMediator>(),
				scopedProvider.GetRequiredService<ITournamentRepository>(),
				scopedProvider.GetRequiredService<IMatchRepository>(),
				scopedProvider.GetRequiredService<IMatchGenerator>(),
				lastRun,
				now).ExecuteAsync(cancellationToken);
		}

		internal class Job
		{
			private readonly DateTime lastRun;
			private readonly ILogger logger;
			private readonly IMatchGenerator matchGenerator;
			private readonly IMatchRepository matchRepository;
			private readonly IMediator mediator;
			private readonly DateTime now;
			private readonly ITournamentRepository tournamentRepository;

			/// <inheritdoc />
			public Job(ILogger logger, IMediator mediator,
				ITournamentRepository tournamentRepository,
				IMatchRepository matchRepository, IMatchGenerator matchGenerator, DateTime lastRun,
				DateTime now)
			{
				this.logger = logger;
				this.tournamentRepository = tournamentRepository;
				this.matchRepository = matchRepository;
				this.matchGenerator = matchGenerator;
				this.lastRun = lastRun;
				this.now = now;
				this.mediator = mediator;
			}

			public async Task ExecuteAsync(CancellationToken cancellationToken)
			{
				await TransferTournamentsToRunning(cancellationToken);
				await GenerateMatchesAsync(cancellationToken);
				await TransferTournamentsToFinished(cancellationToken);
			}

			private async Task GenerateMatchesAsync(CancellationToken cancellationToken)
			{
				// brackets
				{
					var tournaments =
						await tournamentRepository.GetBracketTournamentsForMatchGenerationAsync(
							lastRun,
							cancellationToken);

					foreach (var tournament in tournaments)
					{
						var (matches, done) = matchGenerator.GenerateBrackets(tournament);
						await CreateMatches(cancellationToken, tournament.Id, matches);

						if (done)
						{
							// mark that all matches have been generated
							await MarkGenerationDone(tournament.Id, cancellationToken);
						}
					}
				}

				// deadline
				{
					var tournaments =
						await tournamentRepository.GetDeadlineTournamentsForMatchGenerationAsync(
							cancellationToken);

					foreach (var tournament in tournaments)
					{
						var matches = matchGenerator.GenerateDeadline(tournament);
						await CreateMatches(cancellationToken, tournament.Id, matches);
						await MarkGenerationDone(tournament.Id, cancellationToken);
					}
				}

				// ongoing
				{
					var tournaments =
						await tournamentRepository.GetOngoingTournamentsForMatchGenerationAsync(
							cancellationToken);

					foreach (var tournament in tournaments)
					{
						var toGenerate = tournament.MatchesPerDay *
							(now - tournament.EvaluationStarted).TotalDays -
							tournament.MatchesCount;

						if (toGenerate < 1)
						{
							continue;
						}

						var matches = matchGenerator.GenerateOngoing(tournament, (int)toGenerate);
						await CreateMatches(cancellationToken, tournament.Id, matches);
					}
				}
			}

			private Task CreateMatches(CancellationToken cancellationToken, long tournamentId,
				List<NewMatchDto> matches)
			{
				logger.TournamentMatchesGenerated(tournamentId, matches.Count);
				return matchRepository.CreateMatchesAsync(matches, cancellationToken);
			}

			private async Task MarkGenerationDone(long tournamentId,
				CancellationToken cancellationToken)
			{
				var updateDto = new TournamentStateUpdateDto(TournamentState.WaitingForFinish);
				await tournamentRepository.UpdateAsync(tournamentId, updateDto, cancellationToken);

				logger.TournamentStateChanged(tournamentId, updateDto.State);
			}

			private async Task TransferTournamentsToRunning(CancellationToken cancellationToken)
			{
				var tournaments = await tournamentRepository.GetTournamentsStateInfoAsync(
					new[] {TournamentState.Published},
					cancellationToken);

				var updateDto = new TournamentStartedUpdateDto(now);

				foreach (var tournament in tournaments)
				{
					if (tournament.Deadline.HasValue && tournament.Deadline.Value < now)
					{
						await tournamentRepository.UpdateAsync(tournament.Id, updateDto,
							cancellationToken);

						logger.TournamentStateChanged(tournament.Id, updateDto.State);
					}
				}
			}

			private async Task TransferTournamentsToFinished(CancellationToken cancellationToken)
			{
				var events = await tournamentRepository.ListAsync(
					t => t.Scope == TournamentScope.Deadline &&
						t.State == TournamentState.WaitingForFinish &&
						t.Matches.All(m
							=> m.LastExecution.ExecutorResult == EntryPointResult.Success),
					t => new TournamentFinished(t.Id, t.Name), cancellationToken);

				var updateDto = new TournamentFinishedUpdateDto(now);

				foreach (var e in events)
				{
					await mediator.Publish(e, cancellationToken);
					await tournamentRepository.UpdateAsync(e.TournamentId, updateDto,
						cancellationToken);

					logger.TournamentStateChanged(e.TournamentId, updateDto.State);
				}
			}
		}
	}
}