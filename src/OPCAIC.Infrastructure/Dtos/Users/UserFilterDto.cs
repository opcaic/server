namespace OPCAIC.Infrastructure.Dtos.Users
{
	public class UserFilterDto : FilterDtoBase
	{
		public const string SortByUsername = "username";
		public const string SortByEmail = "email";
		public const string SortByCreated = "created";

		public string Email { get; set; }

		public string Username { get; set; }

		public int? UserRole { get; set; }

		public bool? EmailVerified { get; set; }
	}
}