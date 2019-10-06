using System.Collections.Generic;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.Domain.Enums;

namespace OPCAIC.ApiService.Models.Leaderboards
{
	public class LeaderboardModel
	{
		public List<LeaderboardParticipationModel> Participations { get; set; }
		public TournamentReferenceModel Tournament { get; set; }
		public TournamentFormat TournamentFormat { get; set; }
		public TournamentRankingStrategy TournamentRankingStrategy { get; set; }
		public bool Finished { get; set; }
	}
}