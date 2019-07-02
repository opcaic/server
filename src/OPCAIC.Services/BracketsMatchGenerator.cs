using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Repository;
using OPCAIC.Utils;

namespace OPCAIC.Services
{
	internal class BracketsMatchGenerator : IDeadlineMatchGenerator
	{
		private readonly IMatchRepository matchRepository;
		private ILogger logger;

		public BracketsMatchGenerator(ILogger<BracketsMatchGenerator> logger,
			IMatchRepository matchRepository)
		{
			this.logger = logger;
			this.matchRepository = matchRepository;
		}

		/// <inheritdoc />
		public (IEnumerable<Match> matches, bool done) Generate(Tournament tournament)
		{
			var submissions = tournament.GetActiveSubmissions().OrderBy(s => s.Id).ToList();
			var matchTree = MatchTree.GenerateTree(submissions.Count);
			IList<Match> matches = new List<Match>(submissions.Count);
			var queuedMatches = matchRepository.AllMatchesFromTournament(tournament.Id)
				.ToDictionary(m => m.Id);

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

					var firstPlayer = GetLinkedSubmission(matchNode.FirstPlayerLink, queuedMatches, submissions);
					var secondPlayer = GetLinkedSubmission(matchNode.SecondPlayerLink, queuedMatches, submissions);

					if (firstPlayer != null && secondPlayer != null)
					{
						matches.Add(new Match()
						{
							MatchState = MatchState.Waiting,
							Tournament = tournament,
							Participants = new []{firstPlayer, secondPlayer}
						});
					}
				}

				if (!doNextLevel) // no need to traverse next level
				{
					break;
				}
			}

			return (matches, matchTree.MatchNodesById.Count == queuedMatches.Count + matches.Count);
		}

		private static Submission GetLinkedSubmission(MatchTreeLink link, Dictionary<long, Match> matches,
			List<Submission> submissions)
		{
			switch (link.Type)
			{
				case MatchTreeLinkType.Winner:
				case MatchTreeLinkType.Looser:
					var execution = matches.GetValueOrDefault(link.SourceNode.MatchId)?
						.Executions.ArgMaxOrDefault(e => e.Executed);
					return link.Type == MatchTreeLinkType.Winner
						? execution?.BotResults.ArgMaxOrDefault(r => r.Score).Submission
						: execution?.BotResults.ArgMinOrDefault(r => r.Score).Submission;
				case MatchTreeLinkType.Seed:
					return submissions[link.SourceSeed];
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public Submission GetMatchWinner(Match match)
		{
			Debug.Assert(match.Participants.Count == 2);

			return match.Executions.ArgMaxOrDefault(e => e.Executed)?
				.BotResults.ArgMaxOrDefault(r => r.Score)?.Submission;
		}
	}
}
