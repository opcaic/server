using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.Models.Users
{
	public class NewUserModel
	{
		[EmailAddress]
		public string Email { get; set; }

		[StringLength(40, MinimumLength = 1)]
		public string FirstName { get; set; }

		[StringLength(40, MinimumLength = 1)]
		public string LastName { get; set; }

		public string Password { get; set; }
	}
}