using System.Collections.Generic;

namespace OPCAIC.Application.Services
{
	public abstract class MatchTreeBase
	{
		/// <inheritdoc />
		protected MatchTreeBase(IReadOnlyDictionary<long, MatchTreeNode> matchNodesById,
			IReadOnlyList<IReadOnlyList<MatchTreeNode>> levels)
		{
			MatchNodesById = matchNodesById;
			Levels = levels;
		}

		/// <summary>
		///     Individual nodes of the match tree, lookup by their id.
		/// </summary>
		public IReadOnlyDictionary<long, MatchTreeNode> MatchNodesById { get; }

		/// <summary>
		///     Individual nodes of the match tree organized in layers (last layer is the final match).
		/// </summary>
		public IReadOnlyList<IReadOnlyList<MatchTreeNode>> Levels { get; }
	}

	public class SingleEliminationTree : MatchTreeBase
	{
		/// <inheritdoc />
		public SingleEliminationTree(IReadOnlyDictionary<long, MatchTreeNode> matchNodesById,
			IReadOnlyList<IReadOnlyList<MatchTreeNode>> levels, MatchTreeNode final) : base(
			matchNodesById, levels)
		{
			Final = final;
		}

		/// <summary>
		///     The node of the final match.
		/// </summary>
		public MatchTreeNode Final { get; }
	}

	public class DoubleEliminationTree : MatchTreeBase
	{
		/// <inheritdoc />
		public DoubleEliminationTree(IReadOnlyDictionary<long, MatchTreeNode> matchNodesById,
			IReadOnlyList<IReadOnlyList<MatchTreeNode>> levels, IReadOnlyList<IReadOnlyList<MatchTreeNode>> losersLevels,
			MatchTreeNode final, MatchTreeNode winnersBracketFinal, MatchTreeNode losersBracketFinal, MatchTreeNode thirdPlaceMatch, MatchTreeNode secondaryFinal) : base(
			matchNodesById,
			levels)
		{
			WinnersBracketFinal = winnersBracketFinal;
			LosersBracketFinal = losersBracketFinal;
			ThirdPlaceMatch = thirdPlaceMatch;
			SecondaryFinal = secondaryFinal;
			LosersLevels = losersLevels;
			Final = final;
		}

		/// <summary>
		///     Individual nodes of the losers bracket tree organized in layers (last layer is the final match).
		/// </summary>
		public IReadOnlyList<IReadOnlyList<MatchTreeNode>> LosersLevels { get; }

		/// <summary>
		///     The node of the final match between winners of losers and winners bracket. If winner of
		///     losers bracket wins, then this match needs to be done again to ensure fairness.
		/// </summary>
		public MatchTreeNode Final { get; }

		/// <summary>
		///     The node of the final match between winners of losers and winners bracket. If winner of
		///     losers bracket wins, then this match needs to be done again to ensure fairness.
		/// </summary>
		public MatchTreeNode SecondaryFinal { get; }

		/// <summary>
		///     Match for the third place.
		/// </summary>
		public MatchTreeNode ThirdPlaceMatch { get; }

		/// <summary>
		///     Final match in the winners bracket.
		/// </summary>
		public MatchTreeNode WinnersBracketFinal { get; }

		/// <summary>
		///     Final match in the losers bracket.
		/// </summary>
		public MatchTreeNode LosersBracketFinal { get; }
	}
}