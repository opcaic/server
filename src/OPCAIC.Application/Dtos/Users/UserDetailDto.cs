using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Users.Model;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enumerations;

namespace OPCAIC.Application.Dtos.Users
{
	public class UserDetailDto : UserPreviewDto, IMapFrom<User>
	{
		public string Organization { get; set; }

		public LocalizationLanguage LocalizationLanguage { get; set; }

		public bool WantsEmailNotifications { get; set; }
	}
}