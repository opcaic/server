using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models.Users;

namespace OPCAIC.ApiService.Models.Leaderboards
{
	public class LeaderboardMatchModel
	{
		public long Index { get; set; }
		public UserLeaderboardViewModel FirstPlayer { get; set; }
		public UserLeaderboardViewModel SecondPlayer { get; set; }
		public bool FirstPlayerWon { get; set; }
		public double FirstPlayerScore { get; set; }
		public double SecondPlayerScore { get; set; }
	}
}
