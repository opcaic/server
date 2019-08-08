using OPCAIC.ApiService.ModelValidationHandling.Attributes;

namespace OPCAIC.ApiService.Models.Users
{
	public class UserFilterModel
	{
		[ApiRequired]
		[ApiMinValue(0)]
		public int Offset { get; set; }

		[ApiRequired]
		[ApiRange(1, 100)]
		public int Count { get; set; }

		[ApiMinLength(1)]
		public string SortBy { get; set; }

		public bool Asc { get; set; }

		[ApiMinLength(1)]
		public string Email { get; set; }

		[ApiMinLength(1)]
		public string Username { get; set; }

		public UserRole? UserRole { get; set; }

		public bool? EmailVerified { get; set; }
	}
}