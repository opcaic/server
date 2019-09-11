using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Extensions;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Services;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Services
{
	public class TournamentProcessor : HostedJob
	{
		private DateTime lastUpdated = DateTime.MinValue;

		/// <inheritdoc />
		public TournamentProcessor(IServiceProvider serviceProvider,
			ILogger<TournamentProcessor> logger)
			: base(serviceProvider, logger, TimeSpan.FromSeconds(5))
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
				scopedProvider.GetRequiredService<ITournamentRepository>(),
				scopedProvider.GetRequiredService<IMatchRepository>(),
				scopedProvider.GetRequiredService<IMatchGenerator>(),
				lastRun,
				now).ExecuteAsync(cancellationToken);
		}

		internal class Job
		{
			private readonly ILogger logger;
			private readonly IMatchGenerator matchGenerator;
			private readonly IMatchRepository matchRepository;
			private readonly ITournamentRepository tournamentRepository;

			private readonly DateTime lastRun;
			private readonly DateTime now;

			/// <inheritdoc />
			public Job(ILogger logger, ITournamentRepository tournamentRepository, IMatchRepository matchRepository,
				IMatchGenerator matchGenerator, DateTime lastRun, DateTime now)
			{
				this.logger = logger;
				this.tournamentRepository = tournamentRepository;
				this.matchRepository = matchRepository;
				this.matchGenerator = matchGenerator;
				this.lastRun = lastRun;
				this.now = now;
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
						await tournamentRepository.GetBracketTournamentsForMatchGenerationAsync(lastRun,
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

						var matches = matchGenerator.GenerateOngoing(tournament, (int) toGenerate);
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
				await tournamentRepository.UpdateTournamentState(tournamentId,
					new TournamentStateUpdateDto
					{
						State = TournamentState.WaitingForFinish,
					}, cancellationToken);

				logger.TournamentStateChanged(tournamentId, TournamentState.WaitingForFinish);
			}

			private async Task TransferTournamentsToRunning(CancellationToken cancellationToken)
			{
				var tournaments = await tournamentRepository.GetTournamentsStateInfoAsync(
					new[] { TournamentState.Published },
					cancellationToken);

				var updateDto = new TournamentStartedUpdateDto
				{
					State = TournamentState.Running,
					EvaluationStarted = now
				};

				foreach (var tournament in tournaments)
				{
					if (tournament.Deadline.HasValue && tournament.Deadline.Value < now)
					{
						await tournamentRepository.UpdateTournamentState(tournament.Id, updateDto,
							cancellationToken);

						logger.TournamentStateChanged(tournament.Id, updateDto.State);
					}
				}
			}

			private async Task TransferTournamentsToFinished(CancellationToken cancellationToken)
			{
				var tournaments = await tournamentRepository.GetTournamentsForFinishing(cancellationToken);

				var updateDto = new TournamentFinishedUpdateDto
				{
					State = TournamentState.Finished,
					EvaluationFinished = now
				};

				foreach (var tournament in tournaments)
				{
					await tournamentRepository.UpdateTournamentState(tournament.Id, updateDto,
						cancellationToken);

					logger.TournamentStateChanged(tournament.Id, updateDto.State);
				}
			}
		}
	}
}