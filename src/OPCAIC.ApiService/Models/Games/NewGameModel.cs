using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.Models.Games
{
	public class NewGameModel
	{
		[Required, MinLength(1)]
		public string Name { get; set; }
	}
}