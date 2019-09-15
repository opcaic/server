namespace OPCAIC.Infrastructure.Dtos.Users
{
	public class UserProfileDto
	{
		public string LocalizationLanguage { get; set; }

		public string Organization { get; set; }

		public bool WantsEmailNotifications { get; set; }
	}
}