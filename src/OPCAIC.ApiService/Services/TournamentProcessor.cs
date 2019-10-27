using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces.MatchGeneration;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Tournaments.Events;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.ApiService.Services
{
	public class TournamentProcessor : HostedJob
	{
		private DateTime lastUpdated = DateTime.MinValue;
		private readonly ITimeService time;

		/// <inheritdoc />
		public TournamentProcessor(IServiceScopeFactory scopeFactory,
			ILogger<TournamentProcessor> logger, ITimeService time)
			: base(scopeFactory, logger, TimeSpan.FromSeconds(5))
		{
			this.time = time;
		}

		/// <inheritdoc />
		protected override Task ExecuteJob(IServiceProvider scopedProvider,
			CancellationToken cancellationToken)
		{
			var now = time.Now;
			var lastRun = lastUpdated;
			lastUpdated = now;
			return scopedProvider.GetRequiredService<Job>().ExecuteAsync(lastRun, cancellationToken);
		}

		internal class Job
		{
			private readonly ILogger logger;
			private readonly IMatchGenerator matchGenerator;
			private readonly IMatchRepository matchRepository;
			private readonly IMediator mediator;
			private readonly DateTime now;
			private readonly ITournamentRepository tournamentRepository;
			private readonly IMapper mapper;

			/// <inheritdoc />
			public Job(ILogger<TournamentProcessor> logger, IMediator mediator,
				ITournamentRepository tournamentRepository,
				IMatchRepository matchRepository, IMatchGenerator matchGenerator,
				IMapper mapper, ITimeService time)
			{
				this.logger = logger;
				this.tournamentRepository = tournamentRepository;
				this.matchRepository = matchRepository;
				this.matchGenerator = matchGenerator;
				now = time.Now;
				this.mapper = mapper;
				this.mediator = mediator;
			}

			public async Task ExecuteAsync(DateTime lastRun, CancellationToken cancellationToken)
			{
				await TransferTournamentsToRunning(cancellationToken);
				await GenerateMatchesAsync(lastRun, cancellationToken);
				await TransferTournamentsToFinished(cancellationToken);
			}

			private async Task GenerateMatchesAsync(DateTime lastRun, CancellationToken cancellationToken)
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
						await tournamentRepository.ListAsync<Tournament, TournamentOngoingGenerationDto>(t =>
								t.State == TournamentState.Running &&
								t.Scope == TournamentScope.Ongoing &&
								(!t.Matches.Any() ||
									t.Matches.Any(m
										=> m.Executions.Any(e
											=> e.Executed.HasValue && e.Executed > lastRun))),
mapper, cancellationToken);

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
					t => t.State == TournamentState.WaitingForFinish &&
						t.Matches.All(m => m.LastExecution.ExecutorResult == EntryPointResult.Success),
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