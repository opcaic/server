﻿using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.ApiService.Services
{
	public class SubmissionScoreService : ISubmissionScoreService
	{
		private readonly IMapper mapper;
		private readonly ISubmissionRepository submissionRepository;
		private readonly ITournamentRepository tournamentRepository;

		public SubmissionScoreService(IMapper mapper, ISubmissionRepository submissionRepository,
			ITournamentRepository tournamentRepository)
		{
			this.mapper = mapper;
			this.submissionRepository = submissionRepository;
			this.tournamentRepository = tournamentRepository;
		}

		/// <inheritdoc />
		public async Task UpdateSubmissionsScore(MatchExecutionResult result,
			CancellationToken cancellationToken)
		{
			var tournament = await GetTournament(result, cancellationToken);
			switch (tournament.Format)
			{
				case TournamentFormat.SinglePlayer:
					await UpdateSinglePlayerScore(tournament, result, cancellationToken);
					break;
				case TournamentFormat.Table:
					await UpdateTableScore(tournament, result, cancellationToken);
					break;
				case TournamentFormat.Elo:
					await UpdateEloScore(tournament, result, cancellationToken);
					break;
			}
		}

		private async Task UpdateEloScore(TournamentDetailDto tournament, MatchExecutionResult result,
			CancellationToken cancellationToken)
		{
			var sub1Id = result.BotResults[0].SubmissionId;
			var sub2Id = result.BotResults[1].SubmissionId;
			int K = 25;

			var sub1 = await submissionRepository.FindByIdAsync(sub1Id, cancellationToken);
			var sub2 = await submissionRepository.FindByIdAsync(sub2Id, cancellationToken);
			var playerAscore = sub1.Score;
			var playerBscore = sub2.Score;
			var score1 = result.BotResults[0].Score;
			var score2 = result.BotResults[1].Score;

			var ea = GetEloExpectedResult(playerAscore, playerBscore);
			var eb = GetEloExpectedResult(playerBscore, playerAscore);

			await submissionRepository.UpdateSubmissionScoreAsync(sub1Id,
				new UpdateSubmissionScoreDto {Score = playerAscore + K * (score1 - ea)},
				cancellationToken);
			await submissionRepository.UpdateSubmissionScoreAsync(sub2Id,
				new UpdateSubmissionScoreDto {Score = playerBscore + K * (score2 - eb)},
				cancellationToken);
		}

		private async Task UpdateTableScore(TournamentDetailDto tournament, MatchExecutionResult result,
			CancellationToken cancellationToken)
		{
			var sub1Id = result.BotResults[0].SubmissionId;
			var sub2Id = result.BotResults[1].SubmissionId;
			var score1 = result.BotResults[0].Score;
			var score2 = result.BotResults[1].Score;
			await submissionRepository.UpdateSubmissionScoreAsync(sub1Id,
				new UpdateSubmissionScoreDto { Score = score1 + result.BotResults[0].Score },
				cancellationToken);
			await submissionRepository.UpdateSubmissionScoreAsync(sub2Id,
				new UpdateSubmissionScoreDto { Score = score2 + result.BotResults[1].Score },
				cancellationToken);
		}

		private async Task UpdateSinglePlayerScore(TournamentDetailDto tournament,
			MatchExecutionResult result, CancellationToken cancellationToken)
		{
			var subId = result.BotResults[0].SubmissionId;
			var sub = await submissionRepository.FindByIdAsync(subId, cancellationToken);
			var score = result.BotResults[0].Score;
			if (tournament.RankingStrategy == TournamentRankingStrategy.Maximum)
			{
				if (score > sub.Score)
				{
					await submissionRepository.UpdateSubmissionScoreAsync(subId,
						new UpdateSubmissionScoreDto {Score = score},
						cancellationToken);
				}
			}
			else if (tournament.RankingStrategy == TournamentRankingStrategy.Minimum)
			{
				if (score < sub.Score)
				{
					await submissionRepository.UpdateSubmissionScoreAsync(subId,
						new UpdateSubmissionScoreDto {Score = score},
						cancellationToken);
				}
			}
		}

		private async Task<TournamentDetailDto> GetTournament(MatchExecutionResult match,
			CancellationToken cancellationToken)
		{
			var subId = match.BotResults[0].SubmissionId;
			if (!await submissionRepository.ExistsByIdAsync(subId, cancellationToken))
			{
				throw new NotFoundException(nameof(Submission), subId);
			}

			var sub = await submissionRepository.FindByIdAsync(subId, cancellationToken);
			return await tournamentRepository.FindByIdAsync(sub.Tournament.Id, cancellationToken);
		}

		/// <summary>
		///     Get expectation of player A winning.
		/// </summary>
		/// <param name="Ra">Elo of player A.</param>
		/// <param name="Rb">Elo of player B.</param>
		/// <returns></returns>
		public static double GetEloExpectedResult(double Ra, double Rb)
		{
			return 1.0 / (1 + Math.Pow(1.0, (Ra - Rb) / 400));
		}
	}
}