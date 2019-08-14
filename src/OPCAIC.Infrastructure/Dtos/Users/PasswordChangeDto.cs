namespace OPCAIC.Infrastructure.Dtos.Users
{
	public class UserPasswordDto
	{
		public long Id { get; set; }

		public string PasswordHash { get; set; }

		public string PasswordKey { get; set; }
	}
}