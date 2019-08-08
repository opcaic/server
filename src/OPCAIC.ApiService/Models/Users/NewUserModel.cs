using OPCAIC.ApiService.ModelValidationHandling.Attributes;

namespace OPCAIC.ApiService.Models.Users
{
	public class NewUserModel
	{
		[ApiEmailAddress]
		[ApiRequired]
		[ApiMinLength(1)]
		public string Email { get; set; }

		[ApiRequired]
		[ApiMinLength(1)]
		public string Username { get; set; }

		[ApiMinLength(1)]
		public string Organization { get; set; }

		public string LocalizationLanguage { get; set; }

		[ApiRequired]
		[ApiMinLength(6)]
		public string Password { get; set; }
	}
}