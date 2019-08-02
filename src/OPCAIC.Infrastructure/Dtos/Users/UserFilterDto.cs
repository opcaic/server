namespace OPCAIC.Infrastructure.Dtos.Users
{
	public class UserFilterDto
	{
		public const string SortByUsername = "username";
		public const string SortByEmail = "email";
		public const string SortByCreated = "created";

		public int Offset { get; set; }

		public int Count { get; set; }

		public string SortBy { get; set; }

		public bool Asc { get; set; }

		public string Email { get; set; }

		public string Username { get; set; }

		public int? UserRole { get; set; }

		public bool? EmailVerified { get; set; }
	}
}
