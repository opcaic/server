using AutoMapper;
using AutoMapper.Configuration.Annotations;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.ApiService.Models.Users
{
	public class NewUserModel : IMapTo<User>
	{
		public string Email { get; set; }

		public string Username { get; set; }

		public string Organization { get; set; }

		public string LocalizationLanguage { get; set; }

		[IgnoreMap]
		public string Password { get; set; }

		public bool WantsEmailNotifications { get; set; }
	}
}