using System.Collections.Generic;

namespace OPCAIC.Application.Tournaments.Models
{
	class TableLeaderboardDto : TournamentLeaderboardDto
	{
		public List<MatchDto> Matches { get; set; }
	}
}