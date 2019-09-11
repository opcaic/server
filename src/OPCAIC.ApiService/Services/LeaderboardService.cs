﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Models.Leaderboards;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Dtos.Users;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Services;

namespace OPCAIC.ApiService.Services
{
	public class LeaderboardService : ILeaderboardService
	{
		private readonly IMapper mapper;
		private readonly IMatchTreeFactory matchFactory;
		private readonly IMatchRepository matchRepository;
		private readonly ITournamentRepository tournamentRepository;

		public LeaderboardService(IMatchRepository matchRepository,
			ITournamentRepository tournamentRepository,
			IMatchTreeFactory matchFactory,
			IMapper mapper)
		{
			this.matchRepository = matchRepository;
			this.mapper = mapper;
			this.tournamentRepository = tournamentRepository;
			this.matchFactory = matchFactory;
		}

		/// <inheritdoc />
		public async Task<LeaderboardModel> GetTournamentLeaderboard(long tournamentId,
			CancellationToken cancellationToken)
		{
			var tournament =
				await tournamentRepository.FindByIdAsync(tournamentId, cancellationToken);

			if (tournament == null)
			{
				throw new NotFoundException(nameof(Tournament), tournamentId);
			}

			switch (tournament.Format)
			{
				case TournamentFormat.Elo:
					return await GenerateEloTournamentLeaderboard(tournament, cancellationToken);
				case TournamentFormat.DoubleElimination:
					return await GenerateDoubleEliminationTournamentLeaderboard(tournament,
						cancellationToken);
				case TournamentFormat.SingleElimination:
					return await GenerateSingleEliminationTournamentLeaderboard(tournament,
						cancellationToken);
				case TournamentFormat.SinglePlayer:
				case TournamentFormat.Table:
					return await GenerateMatchScoreSumLeaderboard(tournament,
						cancellationToken);
				case TournamentFormat.Unknown:
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#region Elo

		public class EloComputing
		{
			/// <summary>
			///     Computes elo score for all players from all matches.
			/// </summary>
			/// <param name="matches">All matches in the tournament.</param>
			/// <param name="model">Leaderboard model.</param>
			public static void DetermineElo(IEnumerable<MatchDetailDto> matches, LeaderboardModel model)
			{
				// elo coefficient
				int K = 25;
				var matchesList = matches.ToList();
				matchesList.Sort((m1, m2) => m1.Index.CompareTo(m2.Index));
				foreach (var currentMatch in matchesList)
				{
					var match = currentMatch;
					var playerA = model.Participations.Single(p
						=> p.User.Id ==
						match.Executions.Last().BotResults[0].Submission.Author.Id);
					var playerB = model.Participations.Single(p
						=> p.User.Id ==
						currentMatch.Executions.Last().BotResults[1].Submission.Author.Id);
					var ea = GetEloExpectedResult(playerA.Score, playerB.Score);
					var eb = GetEloExpectedResult(playerB.Score, playerA.Score);
					playerA.Score += K * (currentMatch.Executions.Last().BotResults[0].Score - ea);
					playerB.Score += K * (currentMatch.Executions.Last().BotResults[1].Score - eb);
				}
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

		#endregion

		#region Generating leaderboards

		private async Task<LeaderboardModel> GenerateMatchScoreSumLeaderboard(
			TournamentDetailDto tournament, CancellationToken cancellationToken)
		{
			var model = CreateModel<LeaderboardModel>(tournament);
			var matches = await GetMatches(tournament.Id, cancellationToken);

			var players = GetUniquePlayers(matches.Values).ToDictionary(p => p.Id,
				p => new LeaderboardParticipationModel
				{
					User = mapper.Map<UserLeaderboardViewModel>(p)
				});

			foreach (var match in matches.Values)
			{
				var results = match.Executions[match.Executions.Count - 1].BotResults;
				foreach (var result in results)
				{
					players[result.Submission.Author.Id].Score += results[0].Score;
				}
			}

			model.Participations = players.Values.ToList();

			DeterminePlacementFromScore(model);
			return model;
		}

		private async Task<LeaderboardModel> GenerateEloTournamentLeaderboard(
			TournamentDetailDto tournament,
			CancellationToken cancellationToken)
		{
			var model = CreateModel<LeaderboardModel>(tournament);
			var matches = await GetMatches(tournament.Id, cancellationToken);

			foreach (var player in GetUniquePlayers(matches.Values))
			{
				var participation = new LeaderboardParticipationModel
				{
					User = mapper.Map<UserLeaderboardViewModel>(player), Score = 1500
				};
				model.Participations.Add(participation);
			}

			EloComputing.DetermineElo(matches.Values, model);
			DeterminePlacementFromScore(model);
			return model;
		}

		private async Task<SingleEliminationTreeLeaderboardModel>
			GenerateSingleEliminationTournamentLeaderboard(TournamentDetailDto tournament,
				CancellationToken cancellationToken)
		{
			var model = CreateModel<SingleEliminationTreeLeaderboardModel>(tournament);
			var matches = await GetMatches(tournament.Id, cancellationToken);

			// placement makes no sense for unfinished bracket tournaments
			if (!model.Finished) return model;

			var tree = matchFactory.GetSingleEliminationTree(tournament.PlayersCount, false);
			foreach (var player in GetUniquePlayers(matches.Values))
			{
				var participation = new LeaderboardParticipationModel
				{
					User = mapper.Map<UserLeaderboardViewModel>(player)
				};
				model.Participations.Add(participation);
			}

			var final = DetermineSingleEliminationPlacement(matches, tree.Final, model, 1);
			model.Participations.Sort((p1, p2) => p1.Place.CompareTo(p2.Place));
			model.Final = final;
			return model;
		}

		private async Task<DoubleEliminationTreeLeaderboardModel>
			GenerateDoubleEliminationTournamentLeaderboard(TournamentDetailDto tournament,
				CancellationToken cancellationToken)
		{
			var model = CreateModel<DoubleEliminationTreeLeaderboardModel>(tournament);
			var matches = await GetMatches(tournament.Id, cancellationToken);

			model.BracketMatches = new List<BracketMatchModel>();

			// placement makes no sense for unfinished bracket tournaments
			if (!model.Finished) return model;

			var tree = matchFactory.GetDoubleEliminationTree(tournament.ActiveSubmissionsCount);
			foreach (var player in GetUniquePlayers(matches.Values))
			{
				var participation = new LeaderboardParticipationModel
				{
					User = mapper.Map<UserLeaderboardViewModel>(player)
				};
				model.Participations.Add(participation);
			}

			var finalA =
				DetermineDoubleEliminationPlacement(matches, tree.WinnersBracketFinal, model, 2);
			var finalB =
				DetermineDoubleEliminationPlacement(matches, tree.LosersBracketFinal, model, 2);
			model.WinnersBracketFinal = finalA;
			model.LosersBracketFinal = finalB;

			#region Handle final separately

			var finalMatch = matches[tree.Final.MatchIndex];
			var playerA = finalMatch.Submissions[0].Author;
			var playerB = finalMatch.Submissions[1].Author;
			var participationA = model.Participations.Single(p => p.User.Id == playerA.Id);
			var participationB = model.Participations.Single(p => p.User.Id == playerB.Id);
			var firstPlayerWon = finalMatch.Executions.Last().BotResults
					.Single(br => br.Submission.Author.Id == playerA.Id).Score >
				finalMatch.Executions.Last().BotResults
					.Single(br => br.Submission.Author.Id != playerA.Id).Score;

			if (firstPlayerWon)
			{
				participationA.Place = 1;
				participationB.Place = 2;
			}
			else
			{
				participationA.Place = 2;
				participationB.Place = 1;
			}

			model.Final = new BracketMatchModel
			{
				FirstPlayer = mapper.Map<UserLeaderboardViewModel>(playerA),
				SecondPlayer = mapper.Map<UserLeaderboardViewModel>(playerB),
				FirstPlayerWon = firstPlayerWon,
				FirstPlayerOriginMatch = finalA,
				SecondPlayerOriginMatch = finalB
			};
			model.BracketMatches.Add(model.Final);

			#endregion

			model.Participations.Sort((p1, p2) => p1.Place.CompareTo(p2.Place));
			return model;
		}

		#endregion

		#region Auxiliary methods

		/// <summary>
		///     Creates a leaderboard model and initializes it with data from given tournament.
		/// </summary>
		/// <param name="tournament"></param>
		/// <returns>Succesfully executed matches.</returns>
		private TLeaderboards CreateModel<TLeaderboards>(
			TournamentDetailDto tournament)
			where TLeaderboards : LeaderboardModel, new()
		{
			return new TLeaderboards
			{
				Tournament = mapper.Map<TournamentReferenceModel>(tournament),
				TournamentFormat = tournament.Format,
				TournamentRankingStrategy = tournament.RankingStrategy,
				Participations = new List<LeaderboardParticipationModel>(),
				Finished = tournament.State == TournamentState.Finished
			};
		}

		/// <summary>
		///     Gets finished matches from tournament with given id and creates a lookup table by match index inside the tournament;
		/// </summary>
		/// <param name="tournamentId">Id of the tournament</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		private async Task<Dictionary<long, MatchDetailDto>> GetMatches(long tournamentId, CancellationToken cancellationToken)
		{
			var matches =
				await matchRepository.AllMatchesFromTournamentAsync(tournamentId,
					cancellationToken);

			return matches.Where(m => m.State == MatchState.Executed).ToDictionary(m => m.Index);
		}

		/// <summary>
		///     Get unique players that played given matches.
		/// </summary>
		/// <param name="matches"></param>
		/// <returns></returns>
		private static IEnumerable<UserReferenceDto> GetUniquePlayers(IEnumerable<MatchDetailDto> matches)
		{
			return matches.SelectMany(m => m.Submissions.Select(sp => sp.Author))
				.Distinct();
		}

		/// <summary>
		///     Determines placement in the tournament directly from the score (used in ELO, table, single player).
		/// </summary>
		/// <param name="model"></param>
		private void DeterminePlacementFromScore(LeaderboardModel model)
		{
			// TODO: optimize this O(n^2) monstrosity
			foreach (var participation in model.Participations)
			{
				if (model.TournamentRankingStrategy == TournamentRankingStrategy.Minimum)
					participation.Place =
						model.Participations.Count(p => p.Score < participation.Score) + 1;
				else // default is maximum
					participation.Place =
						model.Participations.Count(p => p.Score > participation.Score) + 1;
			}

			model.Participations.Sort((p1, p2) => p1.Place.CompareTo(p2.Place));
		}

		private BracketMatchModel DetermineSingleEliminationPlacement(
			Dictionary<long, MatchDetailDto> matches,
			MatchTreeNode node, SingleEliminationTreeLeaderboardModel model, int playersAbove)
		{
			if (node == null) return null;

			var currentMatch = matches[node.MatchIndex];
			var playerA = currentMatch.Submissions[0].Author;
			var playerB = currentMatch.Submissions[1].Author;
			var participationA = model.Participations.Single(p => p.User.Id == playerA.Id);
			var participationB = model.Participations.Single(p => p.User.Id == playerB.Id);
			var firstPlayerWon = currentMatch.Executions.Last().BotResults
					.Single(br => br.Submission.Author.Id == playerA.Id).Score >
				currentMatch.Executions.Last().BotResults
					.Single(br => br.Submission.Author.Id != playerA.Id).Score;

			// whoever lost, gets placed at playersAbove + 1
			// therefore, the final placement will look like this: 1-2-3 3-5 5 5 5...  
			if (firstPlayerWon)
			{
				participationB.Place = playersAbove + 1;
				// final
				if (playersAbove == 1) participationA.Place = 1;
			}
			else
			{
				participationA.Place = playersAbove + 1;
				if (playersAbove == 1) participationB.Place = 1;
			}

			var current = new BracketMatchModel
			{
				FirstPlayer = mapper.Map<UserLeaderboardViewModel>(playerA),
				SecondPlayer = mapper.Map<UserLeaderboardViewModel>(playerB),
				FirstPlayerWon = firstPlayerWon,
				FirstPlayerOriginMatch = DetermineSingleEliminationPlacement(matches,
					node.FirstPlayerLink.SourceNode, model, playersAbove * 2),
				SecondPlayerOriginMatch = DetermineSingleEliminationPlacement(matches,
					node.SecondPlayerLink.SourceNode, model, playersAbove * 2)
			};

			return current;
		}

		private BracketMatchModel DetermineDoubleEliminationPlacement(Dictionary<long, MatchDetailDto> matches,
			MatchTreeNode node, DoubleEliminationTreeLeaderboardModel model, int playersAbove)
		{
			if (node == null) return null;

			var currentMatch = matches[node.MatchIndex];
			var playerA = currentMatch.Submissions[0].Author;
			var playerB = currentMatch.Submissions[1].Author;
			var participationA = model.Participations.Single(p => p.User.Id == playerA.Id);
			var participationB = model.Participations.Single(p => p.User.Id == playerB.Id);
			var firstPlayerWon = currentMatch.Executions.Last().BotResults
					.Single(br => br.Submission.Author.Id == playerA.Id).Score >
				currentMatch.Executions.Last().BotResults
					.Single(br => br.Submission.Author.Id != playerA.Id).Score;

			// check whether we already processed that match (in a previously done winners bracket)
			var current = model.BracketMatches.SingleOrDefault(bm => bm.MatchId == currentMatch.Id);

			if (current == null)
			{
				if (firstPlayerWon)
				{
					participationB.Place = playersAbove + 1;
				}
				else
				{
					participationA.Place = playersAbove + 1;
				}

				current = new BracketMatchModel
				{
					FirstPlayer = mapper.Map<UserLeaderboardViewModel>(playerA),
					SecondPlayer = mapper.Map<UserLeaderboardViewModel>(playerB),
					FirstPlayerWon = firstPlayerWon,
					FirstPlayerOriginMatch = DetermineDoubleEliminationPlacement(matches,
						node.FirstPlayerLink.SourceNode, model, playersAbove * 2),
					SecondPlayerOriginMatch = DetermineDoubleEliminationPlacement(matches,
						node.SecondPlayerLink.SourceNode, model, playersAbove * 2)
				};
				model.BracketMatches.Add(current);
			}

			return current;
		}

		#endregion
	}
}