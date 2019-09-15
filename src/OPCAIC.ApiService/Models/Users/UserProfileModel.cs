namespace OPCAIC.ApiService.Models.Users
{
	public class UserProfileModel
	{
		public string Organization { get; set; }

		public string LocalizationLanguage { get; set; }

		public bool WantsEmailNotifications { get; set; }
	}
}