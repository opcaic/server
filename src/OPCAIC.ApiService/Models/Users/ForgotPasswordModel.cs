using OPCAIC.ApiService.ModelValidationHandling.Attributes;

namespace OPCAIC.ApiService.Models.Users
{
	public class ForgotPasswordModel
	{
		[ApiRequired]
		[ApiEmailAddress]
		[ApiMinLength(1)]
		public string Email { get; set; }
	}
}