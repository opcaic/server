namespace OPCAIC.ApiService.Models.Users
{
	public class NewUserModel
	{
		public string Email { get; set; }

		public string Username { get; set; }

		public string Organization { get; set; }

		public string LocalizationLanguage { get; set; }

		public string Password { get; set; }

		public bool WantsEmailNotifications { get; set; }
	}
}