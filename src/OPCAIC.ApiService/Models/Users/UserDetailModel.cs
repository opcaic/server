namespace OPCAIC.ApiService.Models.Users
{
	public class UserDetailModel: UserPreviewModel
	{
		public string Organization { get; set; }

		public string LocalizationLanguage { get; set; }
	}
}
