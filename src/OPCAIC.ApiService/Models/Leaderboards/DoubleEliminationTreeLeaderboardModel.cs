using System.Collections.Generic;

namespace OPCAIC.ApiService.Models.Leaderboards
{
	public class DoubleEliminationTreeLeaderboardModel : LeaderboardModel
	{
		public BracketMatchModel Final { get; set; }
		public BracketMatchModel LosersBracketFinal { get; set; }
		public BracketMatchModel WinnersBracketFinal { get; set; }
		public List<BracketMatchModel> BracketMatches { get; set; }
	}
}