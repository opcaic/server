using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.Models.Users
{
	public class UserCredentialsModel
	{
		[Required, EmailAddress]
		public string Email { get; set; }

		[Required]
		public string Password { get; set; }
	}
}