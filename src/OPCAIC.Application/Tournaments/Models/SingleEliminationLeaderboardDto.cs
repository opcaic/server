using System.Collections.Generic;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Tournaments.Models
{
	public class SingleEliminationLeaderboardDto : TournamentLeaderboardDto
	{
		/// <summary>
		///     All matches in the tournament indexed by their <see cref="Match.Index" />.
		/// </summary>
		public Dictionary<long, MatchDto> Matches { get; set; }

		/// <summary>
		///     Levels of the single-elimination bracket tree. Members are indices to the <see cref="" />. The last level contains
		///     both final and third place match. Early levels may contain nulls when byes occur (a player advanced without a
		///     match).
		/// </summary>
		public List<List<long?>> Brackets { get; set; }
	}
}