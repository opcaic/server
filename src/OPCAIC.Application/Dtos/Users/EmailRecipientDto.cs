using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Dtos.Users
{
	public class EmailRecipientDto : IMapFrom<User>
	{
		public string Email { get; set; }

		public string LocalizationLanguage { get; set; }
	}
}