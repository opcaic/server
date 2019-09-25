using AutoMapper;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Infrastructure.AutoMapper;

namespace OPCAIC.ApiService.Models.Leaderboards
{
	public class LeaderboardParticipationModel :ICustomMapping
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

		/// <inheritdoc />
		void ICustomMapping.CreateMapping(Profile configuration)
		{
			configuration.CreateMap<SubmissionDetailDto, LeaderboardParticipationModel>(MemberList.None)
				.ForMember(s => s.User, opt => opt.MapFrom(s => s.Author));
		}
	}
}