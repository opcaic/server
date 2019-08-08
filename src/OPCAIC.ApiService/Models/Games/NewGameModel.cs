using OPCAIC.ApiService.ModelValidationHandling.Attributes;

namespace OPCAIC.ApiService.Models.Games
{
	public class NewGameModel
	{
		[ApiRequired]
		[ApiMinLength(1)]
		public string Name { get; set; }
	}
}