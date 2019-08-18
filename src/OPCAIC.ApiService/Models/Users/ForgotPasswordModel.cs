using OPCAIC.ApiService.ModelValidationHandling.Attributes;

namespace OPCAIC.ApiService.Models.Users
{
	public class ForgotPasswordModel
	{
		[ApiRequired]
		[ApiEmailAddress]
		public string Email { get; set; }
	}
}