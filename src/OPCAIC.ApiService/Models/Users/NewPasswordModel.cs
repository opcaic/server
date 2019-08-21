using System.ComponentModel.DataAnnotations;
using OPCAIC.ApiService.ModelValidationHandling.Attributes;

namespace OPCAIC.ApiService.Models.Users
{
	/// <summary>
	///     Model, which is used to change user's password
	/// </summary>
	public class NewPasswordModel
	{
		[ApiRequired]
		public string OldPassword { get; set; }

		[ApiRequired]
		public string NewPassword { get; set; }
	}
}