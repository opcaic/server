namespace OPCAIC.ApiService.Security
{
	public class UserIdentity
	{
		public long Id { get; set; }

		public string Email { get; set; }

		public string PasswordHash { get; set; }

		public string Token { get; set; }

		public UserRole Role { get; set; }
	}
}