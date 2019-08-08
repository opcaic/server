using OPCAIC.ApiService.ModelValidationHandling.Attributes;

namespace OPCAIC.ApiService.Models.Users
{
	public class UserCredentialsModel
	{
		[ApiRequired]
		[ApiEmailAddress]
		public string Email { get; set; }

		[ApiRequired]
		public string Password { get; set; }
	}
}