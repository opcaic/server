using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.Models.Leaderboards;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Interfaces.MatchGeneration;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Application.Services;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.ApiService.Services
{
	public class LeaderboardService : ILeaderboardService
	{
		private readonly IMapper mapper;
		private readonly IMatchTreeFactory matchFactory;
		private readonly IMatchRepository matchRepository;
		private readonly ISubmissionRepository submissionRepository;
		private readonly ITournamentRepository tournamentRepository;

		public LeaderboardService(IMatchRepository matchRepository,
			ITournamentRepository tournamentRepository,
			ISubmissionRepository submissionRepository,
			IMatchTreeFactory matchFactory,
			IMapper mapper)
		{
			this.matchRepository = matchRepository;
			this.mapper = mapper;
			this.tournamentRepository = tournamentRepository;
			this.matchFactory = matchFactory;
			this.submissionRepository = submissionRepository;
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
				case TournamentFormat.DoubleElimination:
					return await GenerateDoubleEliminationTournamentLeaderboard(tournament,
						cancellationToken);
				case TournamentFormat.SingleElimination:
					return await GenerateSingleEliminationTournamentLeaderboard(tournament,
						cancellationToken);
				case TournamentFormat.SinglePlayer:
					return await GenerateSinglePlayerSubmissionScoreLeaderboard(tournament,
						cancellationToken);
				case TournamentFormat.Table:
				case TournamentFormat.Elo:
					return await GenerateTableSubmissionScoreLeaderboard(tournament,
						cancellationToken);
				case TournamentFormat.Unknown:
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#region Generating leaderboards

		private async Task<LeaderboardModel> GenerateSinglePlayerSubmissionScoreLeaderboard(
			TournamentDetailDto tournament, CancellationToken cancellationToken)
		{
			var model = CreateModel<LeaderboardModel>(tournament);
			foreach (var submission in await submissionRepository.AllSubmissionsFromTournament(
				tournament.Id, cancellationToken))
			{
				model.Participations.Add(mapper.Map<LeaderboardParticipationModel>(submission));
			}

			DeterminePlacementFromScore(model);
			return model;
		}

		private async Task<TableLeaderboardModel> GenerateTableSubmissionScoreLeaderboard(
			TournamentDetailDto tournament, CancellationToken cancellationToken)
		{
			var model = CreateModel<TableLeaderboardModel>(tournament);
			model.Matches = new List<LeaderboardMatchModel>();
			var matches = await GetMatches(tournament.Id, cancellationToken);

			foreach (var match in matches.Values)
			{
				model.Matches.Add(CreateLeaderboardMatchModel<LeaderboardMatchModel>(match));
			}

			foreach (var submission in await submissionRepository.AllSubmissionsFromTournament(
				tournament.Id, cancellationToken))
			{
				model.Participations.Add(mapper.Map<LeaderboardParticipationModel>(submission));
			}

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
			foreach (var submission in await submissionRepository.AllSubmissionsFromTournament(
				tournament.Id, cancellationToken))
			{
				model.Participations.Add(mapper.Map<LeaderboardParticipationModel>(submission));
			}

			var eliminatedAtEliminationTreeLevel = new List<List<LeaderboardParticipationModel>>();
			var final = ProcessSingleEliminationBracket(matches, eliminatedAtEliminationTreeLevel,
				tree.Final, model, 1);

			DetermineEliminationTreePlacement(1, eliminatedAtEliminationTreeLevel);

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
			foreach (var submission in await submissionRepository.AllSubmissionsFromTournament(
				tournament.Id, cancellationToken))
			{
				model.Participations.Add(mapper.Map<LeaderboardParticipationModel>(submission));
			}

			var eliminatedAtEliminationTreeLevel = new List<List<LeaderboardParticipationModel>>();
			var finalA = ProcessWinnersBracket(matches, tree.WinnersBracketFinal, model);
			var final = ProcessLosersBracket(matches, eliminatedAtEliminationTreeLevel,
				tree.Final, model, 1);
			model.WinnersBracketFinal = finalA;
			model.LosersBracketFinal =
				model.BracketMatches.Single(bm => bm.Index == tree.LosersBracketFinal.MatchIndex);
			model.Final = final;
            
			DetermineEliminationTreePlacement(1, eliminatedAtEliminationTreeLevel);
			var winner = model.Participations.Single(p => p.Place == 0);
			winner.Place = 1;

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
		///     Gets finished matches from tournament with given id and creates a lookup table by match index inside the
		///     tournament;
		/// </summary>
		/// <param name="tournamentId">Id of the tournament</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		private async Task<Dictionary<long, MatchDetailDto>> GetMatches(long tournamentId,
			CancellationToken cancellationToken)
		{
			var matches =
				await matchRepository.AllMatchesFromTournamentAsync(tournamentId,
					cancellationToken);

			return matches.Where(m => m.State == MatchState.Executed).ToDictionary(m => m.Index);
		}

		private TLeaderboardMatchModel CreateLeaderboardMatchModel<TLeaderboardMatchModel>(
			MatchDetailDto match)
			where TLeaderboardMatchModel : LeaderboardMatchModel, new()
		{
			var playerA = match.Submissions[0].Author;
			var playerB = match.Submissions[1].Author;
			var firstPlayerScore = match.Executions.Last().BotResults
				.Single(br => br.Submission.Author.Id == playerA.Id).Score;
			var secondPlayerScore = match.Executions.Last().BotResults
				.Single(br => br.Submission.Author.Id == playerB.Id).Score;
			return new TLeaderboardMatchModel
			{
				Index = match.Index,
				FirstPlayer = mapper.Map<UserLeaderboardViewModel>(match.Submissions[0].Author),
				SecondPlayer =
					mapper.Map<UserLeaderboardViewModel>(match.Submissions[1].Author),
				FirstPlayerWon = firstPlayerScore > secondPlayerScore,
				FirstPlayerScore = firstPlayerScore,
				SecondPlayerScore = secondPlayerScore
			};
		}

		/// <summary>
		///     Determines placement in the tournament directly from the score (used in ELO, table, single player).
		/// </summary>
		/// <param name="model"></param>
		private void DeterminePlacementFromScore(LeaderboardModel model)
		{
			// TODO: optimized..?
			if (model.TournamentRankingStrategy == TournamentRankingStrategy.Maximum)
			{
				model.Participations.Sort((p1, p2) => -p1.Score.CompareTo(p2.Score));
			}
			else
			{
				model.Participations.Sort((p1, p2) => p1.Score.CompareTo(p2.Score));
			}

			for (int i = 0; i < model.Participations.Count; i++)
			{
				model.Participations[i].Place =
					i > 0 && model.Participations[i - 1].Score == model.Participations[i].Score
						? model.Participations[i - 1].Place
						: i + 1;
			}
		}

		private BracketMatchModel ProcessSingleEliminationBracket(
			Dictionary<long, MatchDetailDto> matches,
			List<List<LeaderboardParticipationModel>> eliminatedAtEliminationTreeLevel,
			MatchTreeNode node, SingleEliminationTreeLeaderboardModel model, int level)
		{
			if (node == null) return null;

			var currentMatch = matches[node.MatchIndex];
			var current = CreateLeaderboardMatchModel<BracketMatchModel>(currentMatch);

			if (eliminatedAtEliminationTreeLevel.Count < level)
				eliminatedAtEliminationTreeLevel.Add(new List<LeaderboardParticipationModel>());

			if (current.FirstPlayerWon)
			{
				eliminatedAtEliminationTreeLevel[level - 1]
					.Add(model.Participations.Single(p => p.User.Id == current.SecondPlayer.Id));
				if (level == 1)
				{
					model.Participations.Single(p => p.User.Id == current.FirstPlayer.Id).Place = 1;
				}
			}
			else
			{
				eliminatedAtEliminationTreeLevel[level - 1]
					.Add(model.Participations.Single(p => p.User.Id == current.FirstPlayer.Id));
				if (level == 1)
				{
					model.Participations.Single(p => p.User.Id == current.SecondPlayer.Id).Place =
						1;
				}
			}
            
			current.FirstPlayerOriginMatch = ProcessSingleEliminationBracket(matches,
				eliminatedAtEliminationTreeLevel,
				node.FirstPlayerLink.SourceNode, model, level + 1);
			current.SecondPlayerOriginMatch = ProcessSingleEliminationBracket(matches,
				eliminatedAtEliminationTreeLevel,
				node.SecondPlayerLink.SourceNode, model, level + 1);

			return current;
		}

		private BracketMatchModel ProcessWinnersBracket(Dictionary<long, MatchDetailDto> matches,
			MatchTreeNode node, DoubleEliminationTreeLeaderboardModel model)
		{
			if (node == null) return null;

			var currentMatch = matches[node.MatchIndex];
			var current = CreateLeaderboardMatchModel<BracketMatchModel>(currentMatch);

			current.FirstPlayerOriginMatch = ProcessWinnersBracket(matches,
				node.FirstPlayerLink.SourceNode, model);
			current.SecondPlayerOriginMatch = ProcessWinnersBracket(matches,
				node.SecondPlayerLink.SourceNode, model);
			model.BracketMatches.Add(current);

			return current;
		}

		private BracketMatchModel ProcessLosersBracket(Dictionary<long, MatchDetailDto> matches,
			List<List<LeaderboardParticipationModel>> eliminatedAtEliminationTreeLevel,
			MatchTreeNode node, DoubleEliminationTreeLeaderboardModel model, int level)
		{
			if (node == null) return null;

			var currentMatch = matches[node.MatchIndex];
			var current = CreateLeaderboardMatchModel<BracketMatchModel>(currentMatch);
            
			if (eliminatedAtEliminationTreeLevel.Count < level)
				eliminatedAtEliminationTreeLevel.Add(new List<LeaderboardParticipationModel>());

			if (current.FirstPlayerWon)
				eliminatedAtEliminationTreeLevel[level - 1]
					.Add(model.Participations.Single(p => p.User.Id == current.SecondPlayer.Id));
			else
				eliminatedAtEliminationTreeLevel[level - 1]
					.Add(model.Participations.Single(p => p.User.Id == current.FirstPlayer.Id));

			if (node.FirstPlayerLink.SourceNode != null)
			{
				var firstPlayerOriginMatch = model.BracketMatches.SingleOrDefault(bm
					=> bm.Index == node.FirstPlayerLink.SourceNode.MatchIndex);
				current.FirstPlayerOriginMatch = firstPlayerOriginMatch ?? ProcessLosersBracket(matches,
					eliminatedAtEliminationTreeLevel,
					node.FirstPlayerLink.SourceNode, model,
					level + 1);
			}

			if (node.SecondPlayerLink.SourceNode != null)
			{
				var secondPlayerOriginMatch = model.BracketMatches.SingleOrDefault(bm
					=> bm.Index == node.SecondPlayerLink.SourceNode.MatchIndex);
				current.SecondPlayerOriginMatch = secondPlayerOriginMatch ?? ProcessLosersBracket(matches,
					eliminatedAtEliminationTreeLevel,
					node.SecondPlayerLink.SourceNode, model,
					level + 1);
			}

			model.BracketMatches.Add(current);
			return current;
		}

		private void DetermineEliminationTreePlacement(int playersAbove,
			List<List<LeaderboardParticipationModel>> eliminatedAtEliminationTreeLevel)
		{
			int above = playersAbove;
			for (int i = 0; i < eliminatedAtEliminationTreeLevel.Count; i++)
			{
				foreach (var participation in eliminatedAtEliminationTreeLevel[i])
				{
					participation.Place = above + 1;
				}

				above += eliminatedAtEliminationTreeLevel[i].Count;
			}
		}

		#endregion
	}
}