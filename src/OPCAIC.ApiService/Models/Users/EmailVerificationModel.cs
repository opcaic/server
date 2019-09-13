
namespace OPCAIC.ApiService.Models.Users
{
	/// <summary>
	///     Model, which is used to verify user's email.
	/// </summary>
	public class EmailVerificationModel
	{
		public string Email { get; set; }

		public string Token { get; set; }
	}
}