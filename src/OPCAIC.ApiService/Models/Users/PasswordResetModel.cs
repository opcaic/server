using OPCAIC.ApiService.ModelValidationHandling.Attributes;

namespace OPCAIC.ApiService.Models.Users
{
	public class PasswordResetModel
	{
		[ApiEmailAddress]
		[ApiRequired]
		[ApiMinLength(1)]
		public string Email { get; set; }

		[ApiRequired]
		[ApiMinLength(1)]
		public string ResetToken { get; set; }

		[ApiRequired]
		public string Password { get; set; }
	}
}