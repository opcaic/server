using System.Diagnostics;

namespace OPCAIC.Application.Services
{
	/// <summary>
	///     Link between matches in a match tree.
	/// </summary>
	public class MatchTreeLink
	{
		public MatchTreeLink(MatchTreeNode sourceNode, MatchTreeLinkType type)
		{
			Debug.Assert(type != MatchTreeLinkType.Seed);

			SourceNode = sourceNode;
			Type = type;
		}

		public MatchTreeLink(int sourceSeed)
		{
			SourceSeed = sourceSeed;
			Type = MatchTreeLinkType.Seed;
		}

		/// <summary>
		///     If <see cref="Type" /> is <see cref="MatchTreeLinkType.Winner" /> or
		///     <see cref="MatchTreeLinkType.Looser" />, contains the match whose winner or looser is linked.
		/// </summary>
		public MatchTreeNode SourceNode { get; }

		/// <summary>
		///     If <see cref="Type" /> is <see cref="MatchTreeLinkType.Seed" />, contains the seed of the target
		///     player.
		/// </summary>
		public int SourceSeed { get; }

		/// <summary>
		///     Match to which the linked competitor should continue.
		/// </summary>
		public MatchTreeNode TargetNode { get; set; }

		/// <summary>
		///     Type of the link
		/// </summary>
		public MatchTreeLinkType Type { get; }
	}
}