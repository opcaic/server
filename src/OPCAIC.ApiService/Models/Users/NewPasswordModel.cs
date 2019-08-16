using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.Models.Users
{
	/// <summary>
	///     Model, which is used to change user's password
	/// </summary>
	public class NewPasswordModel
	{
		/// <summary>
		///     Key, which was obtained
		/// </summary>
		public string PasswordKey { get; set; }

		public string OldPassword { get; set; }

		[Required]
		public string NewPassword { get; set; }
	}
}