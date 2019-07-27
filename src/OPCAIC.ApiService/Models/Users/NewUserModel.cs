using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.Models.Users
{
	public class NewUserModel
	{
		[EmailAddress, Required, MinLength(1)]
		public string Email { get; set; }

		[Required, MinLength(1)]
		public string UserName { get; set; }

		[MinLength(1)]
		public string Organization { get; set; }

		public string LocalizationLanguage { get; set; }

		[Required, MinLength(6)]
		public string Password { get; set; }
	}
}