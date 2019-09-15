using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using OPCAIC.Domain.Enums;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Utils;

namespace OPCAIC.Services
{
	/// <summary>
	///     Base class for match generators for brackets tournament format.
	/// </summary>
	public abstract class BracketMatchGenerator
	{
		protected readonly ILogger Logger;
		protected readonly IMatchTreeFactory matchTreeFactory;

		/// <inheritdoc />
		protected BracketMatchGenerator(ILogger logger,
			IMatchTreeFactory matchTreeFactory)
		{
			this.matchTreeFactory = matchTreeFactory;
			Logger = logger;
		}

		/// <summary>
		///     Gets the submission the link points to. May return null if the link points to match and
		///     has not been executed yet.
		/// </summary>
		/// <param name="link">The link to the submission.</param>
		/// <param name="matches">Matches executed so far, lookup by their index.</param>
		/// <param name="submissions">All submissions participating in the match.</param>
		/// <returns></returns>
		private static long? GetLinkedSubmission(MatchTreeLink link,
			Dictionary<long, MatchDetailDto> matches,
			List<long> submissions)
		{
			switch (link.Type)
			{
				case MatchTreeLinkType.Winner:
				case MatchTreeLinkType.Looser:
					var execution = matches.GetValueOrDefault(link.SourceNode.MatchIndex)?
						.Executions.OrderBy(e => e.Created).First();

					if (execution?.ExecutorResult != EntryPointResult.Success)
					{
						return null; // either not executed or error
					}

					return link.Type == MatchTreeLinkType.Winner
						? execution.BotResults.ArgMaxOrDefault(r => r.Score).Submission.Id
						: execution.BotResults.ArgMinOrDefault(r => r.Score).Submission.Id;
				case MatchTreeLinkType.Seed:
					return submissions[link.SourceSeed];
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		///     Generates matches for the given tournament based on the given match tree (regardless of
		///     elimination type).
		/// </summary>
		/// <param name="tournament">The source tournament.</param>
		/// <param name="matchTree">Generated match tree template for given tournament.</param>
		/// <param name="queuedMatches">Matches already queued for execution.</param>
		/// <returns></returns>
		protected List<NewMatchDto> GenerateInternal(TournamentBracketsGenerationDto tournament, MatchTreeBase matchTree,
			Dictionary<long, MatchDetailDto> queuedMatches)
		{
			var submissions = tournament.ActiveSubmissionIds;
			submissions.Sort();
			var matches = new List<NewMatchDto>(submissions.Count);

			foreach (var treeLevel in matchTree.Levels)
			{
				var doNextLevel = false;
				foreach (var matchNode in treeLevel)
				{
					if (matchNode == null || queuedMatches.ContainsKey(matchNode.MatchIndex))
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
						matches.Add(new NewMatchDto()
						{
							Index = matchNode.MatchIndex,
							TournamentId = tournament.Id,
							Submissions = new List<long>(2)
							{
								firstPlayer.Value,
								secondPlayer.Value
							}
						});
					}
				}

				if (!doNextLevel
				) // no need to traverse next level, no more matches can be generated
				{
					break;
				}
			}

			return matches;
		}

		/// <summary>
		///     Gets reference <see cref="SubmissionReferenceDto" /> to the winner of the match or null if the match has not been
		///     executed yet.
		/// </summary>
		/// <param name="match">The match.</param>
		/// <returns></returns>
		protected static SubmissionReferenceDto GetMatchWinner(MatchDetailDto match)
		{
			Debug.Assert(match.Submissions.Count == 2);

			return match.Executions?.OrderBy(e => e.Created).FirstOrDefault()?.BotResults
				.ArgMax(r => r.Score).Submission;
		}
	}

	/// <summary>
	///     Match generator for the double-elimination bracket tournament format.
	/// </summary>
	public class DoubleEliminationMatchGenerator : BracketMatchGenerator, IBracketsMatchGenerator
	{
		/// <inheritdoc />
		public DoubleEliminationMatchGenerator(ILogger<DoubleEliminationMatchGenerator> logger, IMatchTreeFactory matchTreeFactory) : base(logger, matchTreeFactory)
		{
		}

		/// <inheritdoc />
		public TournamentFormat Format => TournamentFormat.DoubleElimination;

		/// <inheritdoc />
		public (List<NewMatchDto> matches, bool done) Generate(
			TournamentBracketsGenerationDto tournament)
		{
			Require.That<ArgumentException>(Format == tournament.Format, "Wrong tournament format");
			var tree = matchTreeFactory.GetDoubleEliminationTree(tournament.ActiveSubmissionIds.Count);
			var queued = tournament.Matches.ToDictionary(m => m.Index);

			if (tree.Final == null)
			{
				return (new List<NewMatchDto>(), true); // edge case
			}

			if (queued.TryGetValue(tree.Final.MatchIndex, out var match) &&
				GetMatchWinner(match) != null)
			{
				// if final has been already finished, check if the additional match is needed
				if (match.Submissions[0].Id != GetMatchWinner(match).Id &&
					queued.Count == tree.MatchNodesById.Count)
				{
					// winner comes from losers bracket, and needs to win twice in order for the tournament to be fair
					// tie breaker already scheduled
					var participations = match.Submissions
						.Select(s => s.Id)
						.Reverse() // switch the sides
						.ToList();

					var m = new NewMatchDto
					{
						Index = tree.MatchNodesById.Count,
						TournamentId = tournament.Id,
						Submissions = participations
					};

					return (new List<NewMatchDto> { m }, false); // last match
				}

				return (new List<NewMatchDto>(), true); // no more matches
			}

			var matches = GenerateInternal(tournament, tree, queued);
			return (matches, false); // we don't know we are finished until the final is played
		}
	}

	/// <summary>
	///     Match generator for the single-elimination (single third place) bracket tournament format.
	/// </summary>
	public class SingleEliminationMatchGenerator : BracketMatchGenerator, IBracketsMatchGenerator
	{
		/// <inheritdoc />
		public SingleEliminationMatchGenerator(ILogger<SingleEliminationMatchGenerator> logger,
			IMatchTreeFactory matchTreeFactory) 
			: base(logger, matchTreeFactory)
		{
		}

		/// <inheritdoc />
		public TournamentFormat Format => TournamentFormat.SingleElimination;

		/// <inheritdoc />
		public (List<NewMatchDto> matches, bool done) Generate(
			TournamentBracketsGenerationDto tournament)
		{
			Require.That<ArgumentException>(Format == tournament.Format, "Wrong tournament format");
			var tree =
				matchTreeFactory.GetSingleEliminationTree(tournament.ActiveSubmissionIds.Count, true);
			var queued = tournament.Matches.ToDictionary(m => m.Index);
			var matches = GenerateInternal(tournament, tree, queued);

			return (matches, tree.MatchNodesById.Count == queued.Count + matches.Count);
		}
	}
}