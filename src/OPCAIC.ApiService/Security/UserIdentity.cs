﻿namespace OPCAIC.ApiService.Security
{
	public class UserIdentity
	{
		public long Id { get; set; }

		public string Email { get; set; }

		public string RefreshToken { get; set; }

		public string AccessToken { get; set; }

		public UserRole Role { get; set; }
	}
}