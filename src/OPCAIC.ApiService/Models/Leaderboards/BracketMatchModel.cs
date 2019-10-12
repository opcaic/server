using OPCAIC.ApiService.Models.Users;

namespace OPCAIC.ApiService.Models.Leaderboards
{
	public class BracketMatchModel : LeaderboardMatchModel
	{
		public BracketMatchModel FirstPlayerOriginMatch { get; set; }
		public BracketMatchModel SecondPlayerOriginMatch { get; set; }
	}
}