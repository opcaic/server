using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using OPCAIC.Infrastructure;

namespace OPCAIC.ApiService.Models.Games
{
	public class NewGameModel
	{
		[ApiRequired]
		[ApiMinLength(1)]
		[ApiMaxLength(StringLengths.GameName)]
		public string Name { get; set; }
	}
}