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
		public string UserName { get; set; }

		[ApiMinLength(1)]
		public string Organization { get; set; }

		[ApiRequired]
		[ApiMinLength(2)]
		[ApiMaxLength(2)]
		public string LocalizationLanguage { get; set; }

		[ApiRequired]
		[ApiMinLength(6)]
		public string Password { get; set; }
	}
}