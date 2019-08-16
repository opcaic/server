namespace OPCAIC.Infrastructure.Dtos.Users
{
	public class UserIdentityDto
	{
		public long Id { get; set; }

		public string Email { get; set; }

		public string RefreshToken { get; set; }

		public string Token { get; set; }

		public long RoleId { get; set; }
	}
}