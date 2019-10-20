using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OPCAIC.Utils;

namespace OPCAIC.Application.Services.MatchGeneration
{
	public static class MatchTreeGenerator
	{
		/// <inheritdoc />
		public static DoubleEliminationTree GenerateDoubleElimination(int seedCount)
		{
			var (_, logSize) = ClosestLargerPowerOfTwo(seedCount);
			var winnerLinks = GenerateSeedIndices(logSize)
				.Select(i => i < seedCount ? new MatchTreeLink(i) : null).ToList();
			var looserLinks = new List<MatchTreeLink>();
			var winnerBracketLevels = new List<List<MatchTreeNode>>();
			var looserBracketLevels = new List<List<MatchTreeNode>>();
			var matchesByIndex = new Dictionary<long, MatchTreeNode>();

			var matchId = 0;

			MatchTreeNode NewNode(MatchTreeLink link0, MatchTreeLink link1)
			{
				var node = new MatchTreeNode(link0, link1, matchId++);
				matchesByIndex.Add(node.MatchIndex, node);
				return node;
			}

			var nextWinnerLinks = new List<MatchTreeLink>(winnerLinks.Count / 2);
			var matches = new List<MatchTreeNode>(winnerLinks.Count / 2);

			// generate first level of the tournament
			for (var i = 0; i < winnerLinks.Count - 1; i += 2)
			{
				Debug.Assert(winnerLinks[i] != null);
				if (winnerLinks[i + 1] != null)
				{
					var match = NewNode(winnerLinks[i], winnerLinks[i + 1]);
					matches.Add(match);
					nextWinnerLinks.Add(match.WinnerLink);
					looserLinks.Add(match.LooserLink);
				}
				else // a bye
				{
					nextWinnerLinks.Add(winnerLinks[i]);
					looserLinks.Add(null);
					matches.Add(null); // to keep things aligned
				}
			}

			winnerBracketLevels.Add(matches);
			matches = new List<MatchTreeNode>();

			// looser bracket
			var count = looserLinks.Count;
			if (count > 1)
			{
				for (var i = 0; i < count; i += 2)
				{
					if (looserLinks[i] != null)
					{
						Debug.Assert(looserLinks[i + 1] != null);
						var match = NewNode(looserLinks[i], looserLinks[i + 1]);
						matches.Add(match);
						looserLinks.Add(match.WinnerLink);
					}
					else // a bye or not enough players (may add null)
					{
						matches.Add(null);
						looserLinks.Add(looserLinks[i + 1]);
					}
				}

				looserLinks.RemoveRange(0, count);
			}

			looserBracketLevels.Add(matches);

			winnerLinks = nextWinnerLinks;

			// generate other levels of tournament tree
			while (winnerLinks.Count > 1)
			{
				nextWinnerLinks = new List<MatchTreeLink>(winnerLinks.Count / 2);
				var nextLooserLinks = new List<MatchTreeLink>(winnerLinks.Count / 2);
				matches = new List<MatchTreeNode>(winnerLinks.Count / 2);

				// winner bracket
				for (var i = 0; i < winnerLinks.Count; i += 2)
				{
					Debug.Assert(winnerLinks[i + 1] != null);
					Debug.Assert(winnerLinks[i] != null);

					var match = NewNode(winnerLinks[i], winnerLinks[i + 1]);
					matches.Add(match);
					nextWinnerLinks.Add(match.WinnerLink);
					nextLooserLinks.Add(match.LooserLink);
				}

				winnerBracketLevels.Add(matches);
				matches = new List<MatchTreeNode>(winnerLinks.Count / 2);

				// looser bracket, minor stage
				Debug.Assert(looserLinks.Count == nextLooserLinks.Count);

				var loosersToEliminate = new List<MatchTreeLink>(looserLinks.Count / 2);
				for (var i = 0; i < looserLinks.Count; i++)
				{
					Debug.Assert(nextLooserLinks[i] != null);

					// Get seed such that two players that did meet in winners bracket do not immediately meet
					// in losers bracket
					var winnerSeed =
						GetLooserBracketSeed(winnerBracketLevels.Count, looserLinks.Count, i);
					if (looserLinks[i] != null)
					{
						var match = NewNode(
							looserLinks[i],
							nextLooserLinks[winnerSeed]); // loser from winners bracket comes second
						matches.Add(match);
						loosersToEliminate.Add(match.WinnerLink);
					}
					else
					{
						matches.Add(null);
						loosersToEliminate.Add(nextLooserLinks[winnerSeed]);
					}
				}

				looserBracketLevels.Add(matches);
				matches = new List<MatchTreeNode>(winnerLinks.Count / 2);

				// looser bracket, major stage
				nextLooserLinks.Clear();
				if (loosersToEliminate.Count > 1)
				{
					for (var i = 0; i < loosersToEliminate.Count; i += 2)
					{
						var match = NewNode(loosersToEliminate[i], loosersToEliminate[i + 1]);
						matches.Add(match);
						nextLooserLinks.Add(match.WinnerLink);
					}

					looserBracketLevels.Add(matches);
				}
				else // last round
				{
					Debug.Assert(nextWinnerLinks.Count == 1);
					nextLooserLinks = loosersToEliminate;
				}

				winnerLinks = nextWinnerLinks;
				looserLinks = nextLooserLinks;
			}

			MatchTreeNode finale = null;
			MatchTreeNode consolation = null;
			MatchTreeNode winnersFinal = null;
			MatchTreeNode losersFinal = null;
			MatchTreeNode secondaryFinal = null;

			// avoid pathological case when there are too few participants
			if (winnerLinks.Count > 0)
			{
				if (seedCount == 2)
				{
                    // the only match is a final
                    finale = winnerBracketLevels[0].Single();
                    winnerBracketLevels.Clear();
                    looserBracketLevels.Clear();
				}
				else
				{
					// winner of the loser brackets vs winner of the winner bracket
					finale = NewNode(winnerLinks[0], looserLinks[0]);
				}

				// secondary final is reversed, but we need to use links from the finale match
				// secondaryFinal = NewNode(looserLinks[0], winnerLinks[0]);
				if (seedCount > 2)
				{
					secondaryFinal = NewNode(finale.WinnerLink, finale.LooserLink);
				}

				winnersFinal = winnerLinks[0].SourceNode;
				losersFinal = looserLinks[0].SourceNode;

				if (seedCount > 3)
				{
					consolation = NewNode(looserBracketLevels[^2].Single().LooserLink,
						looserBracketLevels[^1].Single().LooserLink);
				}
			}

			return new DoubleEliminationTree(
				matchesByIndex,
				winnerBracketLevels,
				looserBracketLevels,
				finale,
				winnersFinal,
				losersFinal,
				consolation,
				secondaryFinal);
		}

		private static int GetLooserBracketSeed(int round, int size, int i)
		{
			Require.Nonnegative(round, nameof(round));

			return round % 2 == 0
				? size - i - 1
				: i;
		}

		public static SingleEliminationTree GenerateSingleElimination(int seedCount,
			bool singleThirdPlace)
		{
			var (size, logSize) = ClosestLargerPowerOfTwo(seedCount);
			var winnerLinks = GenerateSeedIndices(logSize)
				.Select(i => i < seedCount ? new MatchTreeLink(i) : null).ToList();
			var levels = new List<List<MatchTreeNode>>();
			var matchesByIndex = new Dictionary<long, MatchTreeNode>();

			var matchId = 0;

			MatchTreeNode NewNode(MatchTreeLink link0, MatchTreeLink link1)
			{
				var node = new MatchTreeNode(link0, link1, matchId++);
				matchesByIndex.Add(node.MatchIndex, node);
				return node;
			}

			// generate levels of tournament tree
			while (winnerLinks.Count > 1)
			{
				var nextWinnerLinks = new List<MatchTreeLink>(winnerLinks.Count / 2);
				var matches = new List<MatchTreeNode>(winnerLinks.Count / 2);

				for (var i = 0; i < winnerLinks.Count; i += 2)
				{
					Debug.Assert(winnerLinks[i] != null);
					if (winnerLinks[i + 1] != null)
					{
						var match = NewNode(winnerLinks[i], winnerLinks[i + 1]);
						matches.Add(match);
						nextWinnerLinks.Add(match.WinnerLink);
					}
					else // a bye
					{
						// possible only in the leaf level of the tree
						Debug.Assert(winnerLinks.Count == size);
						matches.Add(null);
						nextWinnerLinks.Add(winnerLinks[i]);
					}
				}

				winnerLinks = nextWinnerLinks;
				levels.Add(matches);
			}

			var final = levels.Count > 0 ? levels[^1][0] : null;
			if (singleThirdPlace && seedCount > 3)
			{
				var match = NewNode(
					final.FirstPlayerLink.SourceNode.LooserLink,
					final.SecondPlayerLink.SourceNode.LooserLink);
				levels[^1].Add(match);
			}

			return new SingleEliminationTree(
				matchesByIndex,
				levels,
				final);
		}

		private static (int size, int logSize) ClosestLargerPowerOfTwo(int n)
		{
			var size = 1;
			var logSize = 0;
			while (size < n)
			{
				size <<= 1;
				logSize++;
			}

			return (size, logSize);
		}

		public static int[] GenerateIndicesForCount(int count)
		{
			var (_, log) = ClosestLargerPowerOfTwo(count);
			return GenerateSeedIndices(log);
		}

		/// <summary>
		///     Generates seeds so that if the competitor with lower seeds always wins, then in all stages
		///     the competitor with lowest sourceSeed competes against the competitor with highest sourceSeed. The
		///     second lowest against second highest etc.
		/// </summary>
		/// <param name="logCount"></param>
		/// <returns></returns>
		private static int[] GenerateSeedIndices(int logCount)
		{
			var ret = new Queue<int>(1 << logCount);
			ret.Enqueue(0);
			while (--logCount >= 0)
			{
				var count2 = 2 * ret.Count;
				while (ret.Count < count2)
				{
					var c = ret.Dequeue();
					ret.Enqueue(c);
					ret.Enqueue(count2 - 1 - c);
				}
			}

			return ret.ToArray();
		}
	}
}