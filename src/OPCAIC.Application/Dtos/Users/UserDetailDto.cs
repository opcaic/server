using System;

namespace OPCAIC.Infrastructure.Dtos.Users
{
	public class UserDetailDto : UserPreviewDto
	{
		public string Organization { get; set; }

		public string LocalizationLanguage { get; set; }
	}
}