namespace OPCAIC.ApiService.Models.Users
{
	/// <summary>
	///     Model, which is used to change user's password
	/// </summary>
	public class NewPasswordModel
	{
		public string OldPassword { get; set; }

		public string NewPassword { get; set; }
	}
}