﻿namespace OPCAIC.Infrastructure.Dtos.Users
{
	public class UserPreviewDto
	{
		public string Email { get; set; }

		public string UserName { get; set; }

		public bool EmailVerified { get; set; }

		public long UserRole { get; set; }
	}
}
