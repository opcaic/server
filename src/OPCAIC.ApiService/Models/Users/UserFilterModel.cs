namespace OPCAIC.ApiService.Models.Users
{
	public class UserFilterModel : FilterModelBase
	{
		public string Email { get; set; }

		public string Username { get; set; }

		public UserRole? UserRole { get; set; }

		public bool? EmailVerified { get; set; }
	}
}