namespace OPCAIC.Application.Dtos.Users
{
	public class UserDetailDto : UserPreviewDto
	{
		public string Organization { get; set; }

		public string LocalizationLanguage { get; set; }
	}
}