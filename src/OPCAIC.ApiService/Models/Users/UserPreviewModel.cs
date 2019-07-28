using System;

namespace OPCAIC.ApiService.Models.Users
{
	public class UserPreviewModel
	{
		public long Id { get; set; }

		public string Email { get; set; }

		public string Username { get; set; }

		public bool EmailVerified { get; set; }

		public long UserRole { get; set; }

		public DateTime Created { get; set; }
	}
}
