using OPCAIC.Domain.Enumerations;

namespace OPCAIC.ApiService.Models.Users
{
	public class UserProfileModel
	{
		public string Organization { get; set; }

		public LocalizationLanguage LocalizationLanguage { get; set; }

		public bool WantsEmailNotifications { get; set; }
	}
}