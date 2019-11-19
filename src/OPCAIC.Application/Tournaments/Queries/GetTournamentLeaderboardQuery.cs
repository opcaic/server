using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Interfaces.MatchGeneration;
using OPCAIC.Application.Services;
using OPCAIC.Application.Services.MatchGeneration;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Tournaments.Queries
{
	public class GetTournamentLeaderboardQuery
		: AuthenticatedRequest, IAnonymize, IRequest<TournamentLeaderboardDto>
	{
		public GetTournamentLeaderboardQuery(long tournamentId)
		{
			TournamentId = tournamentId;
		}

		public long TournamentId { get; }

		public bool? Anonymize { get; set; }

		public class Handler
			: IRequestHandler<GetTournamentLeaderboardQuery, TournamentLeaderboardDto>
		{
			private readonly IMapper mapper;
			private readonly IRepository<Match> matchRepository;
			private readonly IRepository<Tournament> repository;
			private readonly IMatchTreeFactory treeFactory;

			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<Tournament> repository,
				IRepository<Match> matchRepository, IMatchTreeFactory treeFactory)
			{
				this.mapper = mapper;
				this.repository = repository;
				this.matchRepository = matchRepository;
				this.treeFactory = treeFactory;
			}

			/// <inheritdoc />
			public async Task<TournamentLeaderboardDto> Handle(
				GetTournamentLeaderboardQuery request, CancellationToken cancellationToken)
			{
				var data = await repository.GetAsync(request.TournamentId,
					t => new Data
					{
						Dto = new TournamentLeaderboardDto
						{
							Format = t.Format,
							Finished = t.EvaluationFinished != null,
							Participations = t.Participants.Where(p => p.ActiveSubmission != null)
								.Select(p => new TournamentLeaderboardDto.ParticipationDto
								{
									SubmissionId = p.ActiveSubmission.Id,
									Author = p.UserId.HasValue? new UserReferenceDto
									{
										Id = p.UserId.Value,
										Organization = p.User.Organization,
										Username = p.User.UserName
									} : null,
									SubmissionScore = p.ActiveSubmission.Score
								}).ToList(),
							RankingStrategy = t.RankingStrategy
						},
						Anonymize = t.Anonymize,
						CanOverrideAnonymize = t.OwnerId == request.RequestingUserId ||
							t.Managers.Any(m => m.UserId == request.RequestingUserId)
					}, cancellationToken);

				var shouldAnonymize = data.CanOverrideAnonymize
					? request.Anonymize ?? data.Anonymize
					: data.Anonymize;

				int slot = 1;
				foreach (var p in data.Dto.Participations.OrderBy(p => p.SubmissionId))
				{
					p.StartingSlot = slot++;

					if (shouldAnonymize && p.Author?.Id != request.RequestingUserId)
					{
						p.Author = UserReferenceDto.Anonymous;
					}
				}

				switch (data.Dto.Format)
				{
					case TournamentFormat.SingleElimination:
						return await CreateSingleElimination(request.TournamentId, data.Dto,
							cancellationToken);
					case TournamentFormat.DoubleElimination:
						return await CreateDoubleElimination(request.TournamentId, data.Dto,
							cancellationToken);
					case TournamentFormat.Table:
						return await CreateTable(request.TournamentId, data.Dto, cancellationToken);
					case TournamentFormat.SinglePlayer:
					case TournamentFormat.Elo:
						DeterminePlacementFromScore(data.Dto);
						return data.Dto; // nothing more needed
					case TournamentFormat.Unknown:
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			private async Task<Dictionary<long, TournamentLeaderboardDto.MatchDto>> GetMatches(
				long tournamentId, CancellationToken cancellationToken)
			{
				var dict = new Dictionary<long, TournamentLeaderboardDto.MatchDto>();

				var matches =
					await matchRepository.ListAsync<Match, MatchData>(
						m => m.TournamentId == tournamentId, mapper, cancellationToken);

				foreach (var data in matches)
				{
					data.LastExecutionBotResults.Sort((a, b) => a.Order.CompareTo(b.Order));
					var players = data.Participations.OrderBy(a => a.Order).Select(p
						=> new TournamentLeaderboardDto.MatchDto.Player
						{
							SubmissionId = p.SubmissionId
						}).ToArray();

					var match = new TournamentLeaderboardDto.MatchDto
					{
						Id = data.Id,
						Executed = data.LastExecutionExecuted,
						Index = data.Index,
						FirstPlayer = players[0],
						SecondPlayer = players[1]
					};

					if (data.LastExecutionExecuted.HasValue)
					{
						for (int i = 0; i < 2; i++)
						{
							players[i].Score = data.LastExecutionBotResults[i].Score;
						}
					}

					dict.Add(data.Index, match);
				}

				return dict;
			}

			private void SetStartingSlotsFromBrackets(
				List<TournamentLeaderboardDto.ParticipationDto> participants)
			{
				var indices = MatchTreeGenerator.GenerateIndicesForCount(participants.Count);
				participants.Sort((a, b) => a.SubmissionId.CompareTo(b.SubmissionId));

				for (int i = 0; i < indices.Length; i++)
				{
					if (indices[i] < participants.Count)
					{
						participants[indices[i]].StartingSlot = i;
					}
				}
			}

			private async Task<SingleEliminationLeaderboardDto> CreateSingleElimination(
				long tournamentId, TournamentLeaderboardDto dto,
				CancellationToken cancellationToken)
			{
				var tree = treeFactory.GetSingleEliminationTree(dto.Participations.Count, true);

				var model = new SingleEliminationLeaderboardDto
				{
					Format = dto.Format,
					Finished = dto.Finished,
					Participations = dto.Participations,
					RankingStrategy = dto.RankingStrategy,
					Matches = await GetMatches(tournamentId, cancellationToken),
					Brackets = tree.Levels.Select(l => l.Select(m => m?.MatchIndex).ToList())
						.ToList()
				};

				SetStartingSlotsFromBrackets(model.Participations);

				if (!model.Finished || model.Participations.Count == 0)
				{
					return model;
				}

				if (model.Participations.Count == 1)
				{
					model.Participations.Single().Place = 1;
				}
				else
				{
					// determine also placement order
					var players = model.Participations.ToDictionary(d => d.SubmissionId);

					// last level may have also "third place final"
					for (var i = 0; i < tree.Levels[^1].Count; i++)
					{
						var match = model.Matches[tree.Levels[^1][i].MatchIndex];
						var (winner, loser) = GetResult(match, model.RankingStrategy);
						players[winner].Place = 2 * i + 1;
						players[loser].Place = 2 * i + 2;
					}

					// for all other levels use appropriate placement for losers. In case there were
					// two matches in the final level, we use 5 the 4 players who lost in
					// semi-finals, 9 for those who lost the level before etc.
					int placement = tree.Levels[^1].Count * 2 + 1;

					// if single third place was specified, then there are two matches in the final
					// level, and we we should skip first 2 levels of the bracket tree to start with
					// level 5, otherwise we want to skip only one
					var skipLevels = tree.Levels[^1].Count;
					DeterminePlacementFromTree(players, model.Matches, model.RankingStrategy,
						placement, tree.Levels.Reverse().Skip(skipLevels));

					model.Participations.Sort((a, b) => a.Place.Value.CompareTo(b.Place.Value));
				}


				return model;
			}

			private void DeterminePlacementFromTree(
				Dictionary<long, TournamentLeaderboardDto.ParticipationDto> players,
				Dictionary<long, TournamentLeaderboardDto.MatchDto> matches,
				TournamentRankingStrategy strategy,
				int firstPlacement, IEnumerable<IEnumerable<MatchTreeNode>> levels)
			{
				var placement = firstPlacement;
				foreach (var level in levels)
				{
					int placementInc = 0;

					foreach (var node in level.Where(m => m != null))
					{
						placementInc++;
						var match = matches[node.MatchIndex];
						var (_, loser) = GetResult(match, strategy);
						players[loser].Place = placement;
					}

					placement += placementInc;
				}
			}

			private async Task<DoubleEliminationLeaderboardDto> CreateDoubleElimination(
				long tournamentId, TournamentLeaderboardDto dto,
				CancellationToken cancellationToken)
			{
				var tree = treeFactory.GetDoubleEliminationTree(dto.Participations.Count);

				var model = new DoubleEliminationLeaderboardDto
				{
					Format = dto.Format,
					Finished = dto.Finished,
					Participations = dto.Participations,
					RankingStrategy = dto.RankingStrategy,
					Matches = await GetMatches(tournamentId, cancellationToken),
					LosersBrackets = GetBracketIndices(tree.LosersLevels),
					WinnersBrackets = GetBracketIndices(tree.Levels),
					FinalIndex = tree.Final?.MatchIndex,
					ConsolationFinalIndex = tree.ThirdPlaceMatch?.MatchIndex
				};

				SetStartingSlotsFromBrackets(model.Participations);

				// avoid edge cases
				if (!model.Finished || model.Participations.Count == 0)
				{
					return model;
				}

				if (model.Participations.Count == 1)
				{
					model.Participations.Single().Place = 1;
					return model;
				}
				// now we have 2 or more participants, so there must be a final
				Debug.Assert(model.FinalIndex.HasValue);

				// If secondary final was needed, fill the index
				if (tree.SecondaryFinal != null && model.Matches.ContainsKey(tree.SecondaryFinal.MatchIndex))
				{
					model.SecondaryFinalIndex = tree.SecondaryFinal.MatchIndex;
				}

				if (model.Finished && model.Participations.Count > 0)
				{
					// determine also placement order
					var players = model.Participations.ToDictionary(d => d.SubmissionId);

					// finals
					{
						var match =
							model.Matches[model.SecondaryFinalIndex ?? model.FinalIndex.Value];
						var (winner, loser) = GetResult(match, model.RankingStrategy);
						players[winner].Place = 1;
						players[loser].Place = 2;
					}

					// third and fourth places
					if (model.ConsolationFinalIndex.HasValue)
					{
						var match =
							model.Matches[model.ConsolationFinalIndex.Value];
						var (winner, loser) = GetResult(match, model.RankingStrategy);
						players[winner].Place = 3;
						players[loser].Place = 4;
					}
					else if (model.Participations.Count == 3)
					{
						// the remaining player has to be third
						players.Values.Single(p => p.Place == null).Place = 3;
					}

					// rest of the placement can be determined from loser brackets
					int placement = 5;
					DeterminePlacementFromTree(players, model.Matches, model.RankingStrategy,
						placement, tree.LosersLevels.Reverse().Skip(2));

					model.Participations.Sort((a, b) => a.Place.Value.CompareTo(b.Place.Value));
				}

				return model;
			}

			private async Task<TournamentLeaderboardDto> CreateTable(long tournamentId,
				TournamentLeaderboardDto dto, CancellationToken cancellationToken)
			{
				var model = new TableLeaderboardDto
				{
					Format = dto.Format,
					RankingStrategy = dto.RankingStrategy,
					Finished = dto.Finished,
					Participations = dto.Participations,
					Matches =
						(await GetMatches(tournamentId, cancellationToken)).Values.ToList()
				};

				DeterminePlacementFromScore(model);

				return model;
			}

			private static (long winner, long loser) GetResult(
				TournamentLeaderboardDto.MatchDto match, TournamentRankingStrategy strategy)
			{
				if (match.FirstPlayer.Score.Value > match.SecondPlayer.Score.Value ==
					(strategy == TournamentRankingStrategy.Maximum))
				{
					return (match.FirstPlayer.SubmissionId, match.SecondPlayer.SubmissionId);
				}

				return (match.SecondPlayer.SubmissionId, match.FirstPlayer.SubmissionId);
			}

			private List<List<long?>> GetBracketIndices(
				IReadOnlyList<IReadOnlyList<MatchTreeNode>> matchNodes)
			{
				return matchNodes.Select(l => l.Select(m => m?.MatchIndex).ToList()).ToList();
			}

			/// <summary>
			///     Determines placement in the tournament directly from the score (used in ELO, table, single player).
			/// </summary>
			/// <param name="dto"></param>
			private static void DeterminePlacementFromScore(TournamentLeaderboardDto dto)
			{
				// TODO: optimized..?
				if (dto.RankingStrategy == TournamentRankingStrategy.Maximum)
				{
					dto.Participations.Sort((p1, p2)
						=> -p1.SubmissionScore.CompareTo(p2.SubmissionScore));
				}
				else
				{
					dto.Participations.Sort((p1, p2)
						=> p1.SubmissionScore.CompareTo(p2.SubmissionScore));
				}

				for (int i = 0; i < dto.Participations.Count; i++)
				{
					dto.Participations[i].Place =
						i > 0 &&
						dto.Participations[i - 1].SubmissionScore ==
						dto.Participations[i].SubmissionScore
							? dto.Participations[i - 1].Place
							: i + 1;
				}
			}

			public class Data
			{
				public TournamentLeaderboardDto Dto { get; set; }
				public bool Anonymize { get; set; }
				public bool CanOverrideAnonymize { get; set; }
			}

			public class MatchData : IMapFrom<Match>
			{
				public long Id { get; set; }

				public long Index { get; set; }

				public DateTime? LastExecutionExecuted { get; set; }

				public List<Player> Participations { get; set; }

				public List<Result> LastExecutionBotResults { get; set; }

				public class Player : IMapFrom<SubmissionParticipation>
				{
					public long SubmissionId { get; set; }

					public int Order { get; set; }
				}

				public class Result : IMapFrom<SubmissionMatchResult>
				{
					public long SubmissionId { get; set; }

					public int Order { get; set; }

					public double Score { get; set; }
				}
			}
		}
	}
}