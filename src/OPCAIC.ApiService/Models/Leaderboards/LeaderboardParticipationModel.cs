using OPCAIC.ApiService.Models.Users;
using OPCAIC.Infrastructure.Dtos.Users;

namespace OPCAIC.ApiService.Models.Leaderboards
{
	public class LeaderboardParticipationModel
	{
		public UserLeaderboardViewModel User { get; set; }

		/// <summary>
		///     The meaning of this value depends on tournament format.
		/// </summary>
		public double Score { get; set; }

		/// <summary>
		///     Placement in the results of the tournament.
		/// </summary>
		public int Place { get; set; }
	}
}