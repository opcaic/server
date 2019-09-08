using OPCAIC.ApiService.Models.Users;

namespace OPCAIC.ApiService.Models.Leaderboards
{
	public class BracketMatchModel
	{
		public UserLeaderboardViewModel FirstPlayer { get; set; }
		public UserLeaderboardViewModel SecondPlayer { get; set; }
		public BracketMatchModel FirstPlayerOriginMatch { get; set; }
		public BracketMatchModel SecondPlayerOriginMatch { get; set; }
		public long MatchId { get; set; }
		public bool? FirstPlayerWon { get; set; }
	}
}