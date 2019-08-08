using OPCAIC.ApiService.ModelValidationHandling.Attributes;

namespace OPCAIC.ApiService.Models.Games
{
	public class UpdateGameModel
	{
		[ApiRequired]
		public string Name { get; set; }
	}
}