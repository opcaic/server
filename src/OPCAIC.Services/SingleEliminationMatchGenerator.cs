﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Utils;

namespace OPCAIC.Services
{
	/// <summary>
	///   Base class for match generators for brackets tournament format.
	/// </summary>
	public abstract class BracketMatchGenerator
	{
		protected readonly ILogger Logger;
		protected readonly IMatchRepository MatchRepository;
		protected readonly IMatchTreeFactory matchTreeFactory;

		/// <inheritdoc />
		protected BracketMatchGenerator(ILogger logger, IMatchRepository matchRepository, IMatchTreeFactory matchTreeFactory)
		{
			MatchRepository = matchRepository;
			this.matchTreeFactory = matchTreeFactory;
			Logger = logger;
		}

		/// <summary>
		///   Gets the submission the link points to. May return null if the link points to match and
		///   has not been executed yet.
		/// </summary>
		/// <param name="link">The link to the submission.</param>
		/// <param name="matches">Matches executed so far.</param>
		/// <param name="submissions">All submissions participating in the match.</param>
		/// <returns></returns>
		private static Submission GetLinkedSubmission(MatchTreeLink link,
			Dictionary<long, Match> matches,
			List<Submission> submissions)
		{
			switch (link.Type)
			{
				case MatchTreeLinkType.Winner:
				case MatchTreeLinkType.Looser:
					var execution = matches.GetValueOrDefault(link.SourceNode.MatchId)?
						.Executions?.ArgMaxOrDefault(e => e.Executed);
					return link.Type == MatchTreeLinkType.Winner
						? execution?.BotResults.ArgMaxOrDefault(r => r.Score).Submission
						: execution?.BotResults.ArgMinOrDefault(r => r.Score).Submission;
				case MatchTreeLinkType.Seed:
					return submissions[link.SourceSeed];
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		///   Generates matches for the given tournament based on the given match tree (regardless of
		///   elimination type).
		/// </summary>
		/// <param name="tournament">The source tournament.</param>
		/// <param name="matchTree">Generated match tree template for given tournament.</param>
		/// <param name="queuedMatches">Matches already queued for execution.</param>
		/// <returns></returns>
		protected List<Match> GenerateInternal(Tournament tournament, MatchTreeBase matchTree,
			Dictionary<long, Match> queuedMatches)
		{
			var submissions = tournament.GetActiveSubmissions().OrderBy(s => s.Id).ToList();
			var matches = new List<Match>(submissions.Count);

			foreach (var treeLevel in matchTree.Levels)
			{
				var doNextLevel = false;
				foreach (var matchNode in treeLevel)
				{
					if (matchNode == null || queuedMatches.ContainsKey(matchNode.MatchId))
					{
						doNextLevel = true;
						continue;
					}

					var firstPlayer =
						GetLinkedSubmission(matchNode.FirstPlayerLink, queuedMatches, submissions);
					var secondPlayer =
						GetLinkedSubmission(matchNode.SecondPlayerLink, queuedMatches, submissions);

					if (firstPlayer != null && secondPlayer != null)
					{
						matches.Add(new Match
						{
							Id = matchNode.MatchId,
							MatchState = MatchState.Waiting,
							Tournament = tournament,
							TournamentId = tournament.Id,
							Participants = new[] {firstPlayer, secondPlayer}
						});
					}
				}

				if (!doNextLevel) // no need to traverse next level, no more matches can be generated
				{
					break;
				}
			}

			return matches;
		}

		/// <summary>
		///   Gets winner <see cref="Submission" /> of the match or null if the match has not been
		///   executed yet.
		/// </summary>
		/// <param name="match">The match.</param>
		/// <returns></returns>
		protected static Submission GetMatchWinner(Match match)
		{
			Debug.Assert(match.Participants.Count == 2);

			return match.Executions?.ArgMaxOrDefault(e => e.Executed)?
				.BotResults.ArgMax(r => r.Score).Submission;
		}
	}

	/// <summary>
	///   Match generator for the double-elimination bracket tournament format.
	/// </summary>
	public class DoubleEliminationMatchGenerator : BracketMatchGenerator, IMatchGenerator
	{
		/// <inheritdoc />
		public TournamentFormat Format => TournamentFormat.DoubleElimination;

		/// <inheritdoc />
		public (IEnumerable<Match> matches, bool done) Generate(Tournament tournament)
		{
			var tree = matchTreeFactory.GetDoubleEliminationTree(tournament.Submissions.Count);
			var queued = MatchRepository.AllMatchesFromTournament(tournament.Id).ToDictionary(m => m.Id);

			if (tree.Final == null)
			{
				return (Enumerable.Empty<Match>(), true); // edge case
			}

			if (queued.TryGetValue(tree.Final.MatchId, out var match) &&
				GetMatchWinner(match) != null)
			{
				// if final has been already finished, check if the additional match is needed
				if (match.Participants[0].Id != GetMatchWinner(match).Id &&
					queued.Count == tree.MatchNodesById.Count)
				{
					// winner comes from losers bracket, and needs to win twice in order for the tournament to be fair
					// tie breaker already scheduled
					var m = new Match
					{
						Id = tree.MatchNodesById.Count,
						MatchState = MatchState.Waiting,
						Tournament = tournament,
						TournamentId = tournament.Id,
						Participants = match.Participants.Reverse().ToList() // switch the sides
					};

					return (Enumerable.Repeat(m, 1), false); // last match
				}

				return (Enumerable.Empty<Match>(), true); // no more matches
			}

			var matches = GenerateInternal(tournament, tree, queued);
			return (matches, false); // we don't know we are finished until the final is played
		}

		/// <inheritdoc />
		public DoubleEliminationMatchGenerator(ILogger<DoubleEliminationMatchGenerator> logger, IMatchRepository matchRepository, IMatchTreeFactory matchTreeFactory) : base(logger, matchRepository, matchTreeFactory)
		{
		}
	}

	/// <summary>
	///   Match generator for the single-elimination (single third place) bracket tournament format.
	/// </summary>
	internal class SingleEliminationMatchGenerator : BracketMatchGenerator, IMatchGenerator
	{
		/// <inheritdoc />
		public TournamentFormat Format => TournamentFormat.SingleElimination;

		/// <inheritdoc />
		public (IEnumerable<Match> matches, bool done) Generate(Tournament tournament)
		{
			var tree = matchTreeFactory.GetSingleEliminationTree(tournament.Submissions.Count, true);
			var queued = MatchRepository.AllMatchesFromTournament(tournament.Id).ToDictionary(m => m.Id);
			var matches = GenerateInternal(tournament, tree, queued);

			return (matches, tree.MatchNodesById.Count == queued.Count + matches.Count);
		}

		/// <inheritdoc />
		public SingleEliminationMatchGenerator(ILogger<SingleEliminationMatchGenerator> logger, IMatchRepository matchRepository, IMatchTreeFactory matchTreeFactory) : base(logger, matchRepository, matchTreeFactory)
		{
		}
	}
}
