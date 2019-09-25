using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Users.Model;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Dtos.Users
{
	public class UserDetailDto : UserPreviewDto, IMapFrom<User>
	{
		public string Organization { get; set; }

		public string LocalizationLanguage { get; set; }
	}
}