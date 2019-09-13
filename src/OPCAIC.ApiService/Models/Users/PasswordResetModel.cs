namespace OPCAIC.ApiService.Models.Users
{
	public class PasswordResetModel
	{
		public string Email { get; set; }

		public string ResetToken { get; set; }

		public string Password { get; set; }
	}
}