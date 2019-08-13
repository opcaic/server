namespace OPCAIC.Infrastructure.Dtos.Users
{
	public class UserPreviewDto
	{
		public long Id { get; set; }

		public string Email { get; set; }

		public string UserName { get; set; }

		public bool EmailVerified { get; set; }

		public long UserRole { get; set; }
	}
}