using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.Models.Leaderboards;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.MatchGeneration;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Services;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.ApiService.Services
{
	public class LeaderboardService : ILeaderboardService
	{
		private readonly IMapper mapper;
		private readonly IMatchTreeFactory matchFactory;
		private readonly IMatchRepository matchRepository;
		private readonly ITournamentRepository tournamentRepository;
		private readonly ISubmissionRepository submissionRepository;

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
				case TournamentFormat.Table:
				case TournamentFormat.Elo:
					return await GenerateSubmissionScoreLeaderboard(tournament,
						cancellationToken);
				case TournamentFormat.Unknown:
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		#region Generating leaderboards

		private async Task<LeaderboardModel> GenerateSubmissionScoreLeaderboard(
			TournamentDetailDto tournament, CancellationToken cancellationToken)
		{
			var model = CreateModel<LeaderboardModel>(tournament);
			foreach (var submission in await submissionRepository.AllSubmissionsFromTournament(tournament.Id, cancellationToken))
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
			foreach (var submission in await submissionRepository.AllSubmissionsFromTournament(tournament.Id, cancellationToken))
			{
				model.Participations.Add(mapper.Map<LeaderboardParticipationModel>(submission)); ;
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
			foreach (var submission in await submissionRepository.AllSubmissionsFromTournament(tournament.Id, cancellationToken))
			{
				model.Participations.Add(mapper.Map<LeaderboardParticipationModel>(submission));
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
					(i > 0 && model.Participations[i - 1].Score == model.Participations[i].Score)
						? model.Participations[i - 1].Place
						: i + 1;
			}
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