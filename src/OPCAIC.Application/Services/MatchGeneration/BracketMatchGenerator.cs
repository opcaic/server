using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Interfaces.MatchGeneration;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Domain.Enums;
using OPCAIC.Utils;

namespace OPCAIC.Application.Services.MatchGeneration
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
		/// <param name="ctx">Contextual data for bracket generation.</param>
		/// <returns></returns>
		private static long? GetLinkedSubmission(MatchTreeLink link, BracketGenerationContext ctx)
		{
			switch (link.Type)
			{
				case MatchTreeLinkType.Winner:
				case MatchTreeLinkType.Looser:
					var execution = ctx.QueuedMatches.GetValueOrDefault(link.SourceNode.MatchIndex)?
						.LastExecution;

					if (execution?.ExecutorResult != EntryPointResult.Success)
					{
						return null; // either not executed or error
					}

					return (link.Type == MatchTreeLinkType.Winner) == (ctx.RankingStrategy == TournamentRankingStrategy.Maximum)
						? execution.BotResults.ArgMaxOrDefault(r => r.Score).Submission.Id
						: execution.BotResults.ArgMinOrDefault(r => r.Score).Submission.Id;
				case MatchTreeLinkType.Seed:
					return ctx.SubmissionIds[link.SourceSeed];
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		protected static void ProcessBrackets(IReadOnlyList<IReadOnlyList<MatchTreeNode>> levels,
			BracketGenerationContext ctx)
		{
			foreach (var treeLevel in levels)
			{
				var doNextLevel = false;
				foreach (var matchNode in treeLevel)
				{
					if (matchNode == null || ctx.QueuedMatches.ContainsKey(matchNode.MatchIndex))
					{
						// a BYE in this level, we might be able to generate match from the next
						// level
						doNextLevel = true;
						continue;
					}

					ProcessNode(matchNode, ctx);
				}

				if (!doNextLevel)
				{
					// no need to traverse next level, no more matches can be generated
					break;
				}
			}
		}

		protected static void ProcessNode(MatchTreeNode matchNode, BracketGenerationContext ctx)
		{
			var firstPlayer =
				GetLinkedSubmission(matchNode.FirstPlayerLink, ctx);
			var secondPlayer =
				GetLinkedSubmission(matchNode.SecondPlayerLink, ctx);

			if (firstPlayer != null && secondPlayer != null)
			{
				ctx.NewMatches.Add(new NewMatchDto
				{
					Index = matchNode.MatchIndex,
					TournamentId = ctx.TournamentId,
					Submissions = new List<long>(2)
					{
						firstPlayer.Value,
						secondPlayer.Value
					}
				});
			}
		}

		/// <summary>
		///     Gets reference <see cref="SubmissionReferenceDto" /> to the winner of the match or null if the match has not been
		///     executed yet.
		/// </summary>
		/// <param name="match">The match.</param>
		/// <param name="strategy">Strategy for choosing the winner based on the score</param>
		/// <returns></returns>
		protected static SubmissionReferenceDto GetMatchWinner(MatchDetailDto match, TournamentRankingStrategy strategy)
		{
			Debug.Assert(match.Submissions.Count == 2);

			var results = match.LastExecution?.BotResults;

			switch (strategy)
			{
				case TournamentRankingStrategy.Maximum:
					return results?.ArgMax(r => r.Score).Submission;
				case TournamentRankingStrategy.Minimum:
					return results?.ArgMin(r => r.Score).Submission;
				case TournamentRankingStrategy.Unknown:
				default:
					throw new ArgumentOutOfRangeException(nameof(strategy), strategy, null);
			}
		}

		public class BracketGenerationContext
		{
			/// <inheritdoc />
			public BracketGenerationContext(List<long> submissionIds, List<NewMatchDto> newMatches, Dictionary<long, MatchDetailDto> queuedMatches, long tournamentId, TournamentRankingStrategy rankingStrategy)
			{
				SubmissionIds = submissionIds;
				NewMatches = newMatches;
				QueuedMatches = queuedMatches;
				TournamentId = tournamentId;
				RankingStrategy = rankingStrategy;
			}

			/// <summary>
			///     Id of the tournament currently being processed.
			/// </summary>
			public long TournamentId { get; }

			/// <summary>
			///     Ranking strategy used for the tournament.
			/// </summary>
			public TournamentRankingStrategy RankingStrategy { get; }

			/// <summary>
			///     Ids of the submissions which should participate in the tournament.
			/// </summary>
			public List<long> SubmissionIds { get; }

			/// <summary>
			///     List of all new matches to be returned from generator.
			/// </summary>
			public List<NewMatchDto> NewMatches { get; }

			/// <summary>
			///     Already generated matches in lookup by the match index.
			/// </summary>
			public Dictionary<long, MatchDetailDto> QueuedMatches { get; }
		}
	}
}