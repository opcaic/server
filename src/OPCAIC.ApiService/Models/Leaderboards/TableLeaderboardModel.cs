using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Models.Leaderboards
{
	public class TableLeaderboardModel : LeaderboardModel
	{
		public List<LeaderboardMatchModel> Matches { get; set; }
	}
}
