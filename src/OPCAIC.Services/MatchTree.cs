using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OPCAIC.Services
{
	public class MatchTree
	{
		/// <inheritdoc />
		private MatchTree(IReadOnlyDictionary<int, MatchTreeNode> matchNodesById,
			IReadOnlyList<IReadOnlyList<MatchTreeNode>> levels, MatchTreeLink firstPlaceLink,
			MatchTreeLink secondPlaceLink, MatchTreeLink thirdPlaceLink, MatchTreeLink thirdPlace2Link)
		{
			MatchNodesById = matchNodesById;
			Levels = levels;
			FirstPlaceLink = firstPlaceLink;
			SecondPlaceLink = secondPlaceLink;
			ThirdPlaceLink = thirdPlaceLink;
			ThirdPlace2Link = thirdPlace2Link;
		}

		public IReadOnlyDictionary<int, MatchTreeNode> MatchNodesById { get; }
		public IReadOnlyList<IReadOnlyList<MatchTreeNode>> Levels { get; }
		public MatchTreeLink FirstPlaceLink { get; }
		public MatchTreeLink SecondPlaceLink { get; }
		public MatchTreeLink ThirdPlaceLink { get; }
		public MatchTreeLink ThirdPlace2Link { get; }

		public static MatchTree GenerateTree(int seedCount)
		{
			var (size, logSize) = ClosestLargerPowerOfTwo(seedCount);
			var links = GenerateSeedIndices(logSize)
				.Select(i => i < seedCount ? new MatchTreeLink(i) : null).ToArray();
			var levels = new List<MatchTreeNode[]>();

			var matchId = 0;

			MatchTreeNode NewNode(MatchTreeLink link0, MatchTreeLink link1)
				=> new MatchTreeNode(link0, link1, matchId++);

			// generate levels of tournament tree
			while (links.Length > 1)
			{
				var nextLinks = new MatchTreeLink[links.Length / 2];
				var matches = new MatchTreeNode[links.Length / 2];

				for (var i = 0; i < links.Length; i += 2)
				{
					Debug.Assert(links[i] != null);
					if (links[i + 1] != null)
					{
						var match = NewNode(links[i], links[i + 1]);
						matches[i / 2] = match;
						nextLinks[i / 2] = match.WinnerLink;
						// TODO: loser link to loser brackets
					}
					else // advanced without a match
					{
						// possible only in the leaf level of the tree
						Debug.Assert(links.Length == size);
						nextLinks[i / 2] = links[i];
					}
				}

				links = nextLinks;
				levels.Add(matches);
			}

			var firstPlaceLink = levels[levels.Count - 1][0].WinnerLink;
			var secondPlaceLink = levels[levels.Count - 1][0].LooserLink;
			var thirdPlaceLink = levels[levels.Count - 2][0].LooserLink;
			var thirdPlace2Link = levels[levels.Count - 2][1].LooserLink;
			// TODO: optional match between the 3rd places

			return new MatchTree(
				levels.SelectMany(i => i).Where(i => i != null).ToDictionary(m => m.MatchId),
				levels,
				firstPlaceLink,
				secondPlaceLink,
				thirdPlaceLink,
				thirdPlace2Link);
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

		/// <summary>
		///   Generates seeds so that if the competitor with lower seeds always wins, then in all stages
		///   the competitor with lowest sourceSeed competes against the competitor with highest sourceSeed. The
		///   second lowest against second highest etc.
		/// </summary>
		/// <param name="logCount">Logarithm of the number of seeds to generate.</param>
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
