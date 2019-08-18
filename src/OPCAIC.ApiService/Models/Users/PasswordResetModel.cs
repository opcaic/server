using OPCAIC.ApiService.ModelValidationHandling.Attributes;

namespace OPCAIC.ApiService.Models.Users
{
	public class PasswordResetModel
	{
		[ApiEmailAddress]
		[ApiRequired]
		public string Email { get; set; }

		[ApiRequired]
		public string ResetToken { get; set; }

		[ApiRequired]
		public string Password { get; set; }
	}
}