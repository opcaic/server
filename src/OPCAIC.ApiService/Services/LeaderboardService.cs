using System;
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
			if (!await tournamentRepository.ExistsByIdAsync(tournamentId, cancellationToken))
			{
				throw new NotFoundException(nameof(Tournament), tournamentId);
			}

			var tournament =
				await tournamentRepository.FindByIdAsync(tournamentId, cancellationToken);
			switch (tournament.Format)
			{
				case TournamentFormat.Elo:
					return await GenerateEloTournamentLeaderboard(tournamentId, cancellationToken);
				case TournamentFormat.DoubleElimination:
					return await GenerateDoubleEliminationTournamentLeaderboard(tournamentId,
						cancellationToken);
				case TournamentFormat.SingleElimination:
					return await GenerateSingleEliminationTournamentLeaderboard(tournamentId,
						cancellationToken);
				case TournamentFormat.SinglePlayer:
					return await GenerateSinglePlayerTournamentLeaderboard(tournamentId,
						cancellationToken);
				case TournamentFormat.Table:
					return await GenerateTableTournamentLeaderboard(tournamentId,
						cancellationToken);
				default:
					return await GenerateTableTournamentLeaderboard(tournamentId,
						cancellationToken);
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

		private async Task<LeaderboardModel> GenerateTableTournamentLeaderboard(
			long tournamentId, CancellationToken cancellationToken)
		{
			var model = new LeaderboardModel();
			var matches = await InitializeLeaderboardModel(model, tournamentId, cancellationToken);
			foreach (var player in GetUniquePlayers(matches))
			{
				var participation =
					new LeaderboardParticipationModel
					{
						User = mapper.Map<UserLeaderboardViewModel>(player)
					};
				int wonMatches = 0;
				int lostMatches = 0;
				int tiedMatches = 0;
				foreach (var m in matches)
				{
					var myResult = m.Executions.Last().BotResults
						.Single(smr => smr.Submission.Author.Id == player.Id);
					var otherResult = m.Executions.Last().BotResults
						.Single(smr => smr.Submission.Author.Id != player.Id);
					if (myResult.Score > otherResult.Score) wonMatches++;
					else if (myResult.Score == otherResult.Score) tiedMatches++;
				}

				// won game is worth 3 points, a tie 1.5 points, loss 0 points
				participation.Score =
					3 * wonMatches + 1.5 * (matches.Count() - lostMatches - wonMatches);
				model.Participations.Add(participation);
			}

			DeterminePlacementFromScore(model);
			return model;
		}

		private async Task<LeaderboardModel> GenerateSinglePlayerTournamentLeaderboard(
			long tournamentId, CancellationToken cancellationToken)
		{
			var model = new LeaderboardModel();
			var matches = await InitializeLeaderboardModel(model, tournamentId, cancellationToken);
			foreach (var player in GetUniquePlayers(matches))
			{
				var participation = new LeaderboardParticipationModel
				{
					User = mapper.Map<UserLeaderboardViewModel>(player), Score = 0
				};
				foreach (var m in matches)
				{
					participation.Score += m.Executions.Last().BotResults
						.Single(br => br.Submission.Author.Id == player.Id).Score;
				}

				model.Participations.Add(participation);
			}

			DeterminePlacementFromScore(model);
			return model;
		}

		private async Task<LeaderboardModel> GenerateEloTournamentLeaderboard(
			long tournamentId,
			CancellationToken cancellationToken)
		{
			var model = new LeaderboardModel();
			var matches = await InitializeLeaderboardModel(model, tournamentId, cancellationToken);
			foreach (var player in GetUniquePlayers(matches))
			{
				var participation = new LeaderboardParticipationModel
				{
					User = mapper.Map<UserLeaderboardViewModel>(player), Score = 1500
				};
				model.Participations.Add(participation);
			}

			EloComputing.DetermineElo(matches, model);
			DeterminePlacementFromScore(model);
			return model;
		}

		private async Task<SingleEliminationTreeLeaderboardModel>
			GenerateSingleEliminationTournamentLeaderboard(long tournamentId,
				CancellationToken cancellationToken)
		{
			var model = new SingleEliminationTreeLeaderboardModel();
			var matches = await InitializeLeaderboardModel(model, tournamentId, cancellationToken);

			// placement makes no sense for unfinished bracket tournaments
			if (!model.Finished) return model;

			var tournament =
				await tournamentRepository.FindByIdAsync(tournamentId, cancellationToken);
			var tree = matchFactory.GetSingleEliminationTree(tournament.PlayersCount, false);
			foreach (var player in GetUniquePlayers(matches))
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
			GenerateDoubleEliminationTournamentLeaderboard(long tournamentId,
				CancellationToken cancellationToken)
		{
			var model = new DoubleEliminationTreeLeaderboardModel();
			var matches = await InitializeLeaderboardModel(model, tournamentId, cancellationToken);
			model.BracketMatches = new List<BracketMatchModel>();

			// placement makes no sense for unfinished bracket tournaments
			if (!model.Finished) return model;

			var tournament =
				await tournamentRepository.FindByIdAsync(tournamentId, cancellationToken);
			var tree = matchFactory.GetDoubleEliminationTree(tournament.PlayersCount);
			foreach (var player in GetUniquePlayers(matches))
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

			var finalMatch = matches.Single(m => m.Index == tree.Final.MatchIndex);
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
		///     Initializes basic leaderboard model, also filters the matches that only those successfully executed are displayed.
		/// </summary>
		/// <param name="model"></param>
		/// <param name="tournamentId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns>Succesfully executed matches.</returns>
		private async Task<IEnumerable<MatchDetailDto>> InitializeLeaderboardModel(
			LeaderboardModel model, long tournamentId, CancellationToken cancellationToken)
		{
			var tournament =
				await tournamentRepository.FindByIdAsync(tournamentId, cancellationToken);
			model.Tournament = mapper.Map<TournamentReferenceModel>(tournament);
			model.TournamentFormat = tournament.Format;
			model.TournamentRankingStrategy = tournament.RankingStrategy;
			model.Participations = new List<LeaderboardParticipationModel>();
			var matches =
				await matchRepository.AllMatchesFromTournamentAsync(tournamentId,
					cancellationToken);
			model.Finished = matches.All(m
					=> m.Executions.Any() &&
					m.Executions.Last().ExecutorResult == EntryPointResult.Success) &&
				matches.Any();
			return matches.Where(m
				=> m.Executions.Any() &&
				m.Executions.Last().ExecutorResult == EntryPointResult.Success);
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
			IEnumerable<MatchDetailDto> matches,
			MatchTreeNode node, SingleEliminationTreeLeaderboardModel model, int playersAbove)
		{
			if (node == null) return null;

			var currentMatch = matches.Single(m => m.Index == node.MatchIndex);
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

		private BracketMatchModel DetermineDoubleEliminationPlacement(IEnumerable<MatchDetailDto> matches,
			MatchTreeNode node, DoubleEliminationTreeLeaderboardModel model, int playersAbove)
		{
			if (node == null) return null;

			var currentMatch = matches.Single(m => m.Index == node.MatchIndex);
			var playerA = currentMatch.Submissions[0].Author;
			var playerB = currentMatch.Submissions[1].Author;
			var participationA = model.Participations.Single(p => p.User.Id == playerA.Id);
			var participationB = model.Participations.Single(p => p.User.Id == playerB.Id);
			var firstPlayerWon = currentMatch.Executions.Last().BotResults
					.Single(br => br.Submission.Author.Id == playerA.Id).Score >
				currentMatch.Executions.Last().BotResults
					.Single(br => br.Submission.Author.Id != playerA.Id).Score;

			// check whether we already processed that match (in a previously done winners bracket)
			var current = model.BracketMatches.Single(bm => bm.MatchId == currentMatch.Id);

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