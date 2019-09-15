namespace OPCAIC.Infrastructure.Dtos.Users
{
	public class NewUserDto
	{
		public string Email { get; set; }

		public string Username { get; set; }

		public string Organization { get; set; }

		public string LocalizationLanguage { get; set; }

		public string PasswordHash { get; set; }

		public int RoleId { get; set; }

		public bool WantsEmailNotifications { get; set; }
	}
}