using System.Collections.Generic;

namespace OPCAIC.Services
{
	public abstract class MatchTreeBase
	{
		/// <inheritdoc />
		protected MatchTreeBase(IReadOnlyDictionary<int, MatchTreeNode> matchNodesById,
			IReadOnlyList<IReadOnlyList<MatchTreeNode>> levels)
		{
			MatchNodesById = matchNodesById;
			Levels = levels;
		}

		/// <summary>
		///   Individual nodes of the match tree, lookup by their id.
		/// </summary>
		public IReadOnlyDictionary<int, MatchTreeNode> MatchNodesById { get; }

		/// <summary>
		///   Individual nodes of the match tree organized in layers (last layer is the final match).
		/// </summary>
		public IReadOnlyList<IReadOnlyList<MatchTreeNode>> Levels { get; }
	}

	public class SingleEliminationTree : MatchTreeBase
	{
		/// <inheritdoc />
		public SingleEliminationTree(IReadOnlyDictionary<int, MatchTreeNode> matchNodesById,
			IReadOnlyList<IReadOnlyList<MatchTreeNode>> levels, MatchTreeNode final) : base(
			matchNodesById, levels)
			=> Final = final;

		/// <summary>
		///   The node of the final match.
		/// </summary>
		public MatchTreeNode Final { get; }
	}

	public class DoubleEliminationTree : MatchTreeBase
	{
		/// <inheritdoc />
		public DoubleEliminationTree(IReadOnlyDictionary<int, MatchTreeNode> matchNodesById,
			IReadOnlyList<IReadOnlyList<MatchTreeNode>> levels, MatchTreeNode final,
			MatchTreeNode winnersBracketFinal, MatchTreeNode losersBracketFinal) : base(matchNodesById,
			levels)
		{
			WinnersBracketFinal = winnersBracketFinal;
			LosersBracketFinal = losersBracketFinal;
			Final = final;
		}

		/// <summary>
		///   The node of the final match between winners of losers and winners bracket. If winner of
		///   losers bracket wins, then this match needs to be done again to ensure fairness.
		/// </summary>
		public MatchTreeNode Final { get; }

		/// <summary>
		///   Final match in the winners bracket.
		/// </summary>
		public MatchTreeNode WinnersBracketFinal { get; }

		/// <summary>
		///   Final match in the losers bracket.
		/// </summary>
		public MatchTreeNode LosersBracketFinal { get; }
	}
}
