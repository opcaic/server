using OPCAIC.ApiService.ModelValidationHandling.Attributes;

namespace OPCAIC.ApiService.Models.Users
{
	/// <summary>
	///     Model, which is used to verify user's email.
	/// </summary>
	public class EmailVerificationModel
	{
		[ApiRequired]
		public string Email { get; set; }

		[ApiRequired]
		public string Token { get; set; }
	}
}