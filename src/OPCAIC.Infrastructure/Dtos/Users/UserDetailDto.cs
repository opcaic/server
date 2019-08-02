using System;

namespace OPCAIC.Infrastructure.Dtos.Users
{
	public class UserDetailDto: UserPreviewDto
	{
		public DateTime Created { get; set; }

		public string Organization { get; set; }

		public string LocalizationLanguage { get; set; }
	}
}
