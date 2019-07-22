using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.Models.Users
{
	public class UserCredentialsModel
	{
		[EmailAddress]
		public string Email { get; set; }

		public string Password { get; set; }
	}
}