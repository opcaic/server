using System.Diagnostics;

namespace OPCAIC.Services
{
	/// <summary>
	///   Class representing a node in a match tree
	/// </summary>
	public class MatchTreeNode
	{
		/// <inheritdoc />
		public MatchTreeNode(MatchTreeLink firstPlayerLink, MatchTreeLink secondPlayerLink, int matchIndex)
		{
			Debug.Assert(firstPlayerLink.TargetNode == null);
			Debug.Assert(secondPlayerLink.TargetNode == null);

			FirstPlayerLink = firstPlayerLink;
			firstPlayerLink.TargetNode = this;
			SecondPlayerLink = secondPlayerLink;
			secondPlayerLink.TargetNode = this;
			MatchIndex = matchIndex;

			WinnerLink = new MatchTreeLink(this, MatchTreeLinkType.Winner);
			LooserLink = new MatchTreeLink(this, MatchTreeLinkType.Looser);
		}

		/// <summary>
		///   Link to where the first player in the matches comes from.
		/// </summary>
		public MatchTreeLink FirstPlayerLink { get; }

		/// <summary>
		///   Link to where the second player in the matches comes from.
		/// </summary>
		public MatchTreeLink SecondPlayerLink { get; }

		/// <summary>
		///   Link to where the winner of this match should continue.
		/// </summary>
		public MatchTreeLink WinnerLink { get; }

		/// <summary>
		///   Link to where the looser of this match should continue.
		/// </summary>
		public MatchTreeLink LooserLink { get; }

		/// <summary>
		///   Unique identifier of the match inside a tournament.
		/// </summary>
		public int MatchIndex { get; }
	}
}
