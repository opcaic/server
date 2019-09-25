using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Infrastructure.AutoMapper;

namespace OPCAIC.ApiService.Models.Users
{
	public class UserLeaderboardViewModel : IMapFrom<UserReferenceDto>
	{
		public string Username { get; set; }
		public string Organization { get; set; }
		public long Id { get; set; }
	}
}