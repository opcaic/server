using System.Collections.Generic;

namespace OPCAIC.Application.Tournaments.Models
{
	public class DoubleEliminationLeaderboardDto : TournamentLeaderboardDto
	{
		/// <summary>
		///     All matches in the tournament indexed by their <see cref="Match.Index" />.
		/// </summary>
		public Dictionary<long, MatchDto> Matches { get; set; }

		/// <summary>
		///     Levels of the winner bracket tree. The last level contains only winner brackets final.
		///     Early levels may contain nulls when byes occur (a player advanced without a
		///     match).
		/// </summary>
		public List<List<long?>> WinnersBrackets { get; set; }

		/// <summary>
		///     Levels of the loser bracket tree. The last level contains only loser brackets final.
		///     Early levels may contain nulls when byes occur (a player advanced without a
		///     match).
		/// </summary>
		public List<List<long?>> LosersBrackets { get; set; }

		/// <summary>
		///     Index of the grand final match between winners of winner and loser bracket. Can be
		///     null if there are too few players.
		/// </summary>
		public long? FinalIndex { get; set; }

		/// <summary>
		///     Optional rematch of Grand final if winner of losers brackets bests the winner of
		///     winner bracket.
		/// </summary>
		public long? SecondaryFinalIndex { get; set; }

		/// <summary>
		///     Index of the match for third and fourth place. Can be null if there is not enouth players.
		/// </summary>
		public long? ConsolationFinalIndex { get; set; }
	}
}