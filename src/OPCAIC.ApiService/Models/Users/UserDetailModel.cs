using OPCAIC.Application.Users.Model;

namespace OPCAIC.ApiService.Models.Users
{
	public class UserDetailModel : UserPreviewDto
	{
		public string Organization { get; set; }

		public string LocalizationLanguage { get; set; }
	}
}