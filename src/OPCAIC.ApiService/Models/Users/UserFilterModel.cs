using OPCAIC.ApiService.Validation;
using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.Models.Users
{
	public class UserFilterModel
	{
		[Required, MinValue(0)]
		public int Offset { get; set; }

		[Required, Range(1, 100)]
		public int Count { get; set; }

		[MinLength(1)]
		public string SortBy { get; set; }

		public bool Asc { get; set; }

		[MinLength(1)]
		public string Email { get; set; }

		[MinLength(1)]
		public string Username { get; set; }

		public UserRole? Role { get; set; }

		public bool? EmailVerified { get; set; }
	}
}
