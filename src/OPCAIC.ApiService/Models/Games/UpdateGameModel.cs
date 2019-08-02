using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.Models.Games
{
	public class UpdateGameModel
	{
		[Required]
		public string Name { get; set; }
	}
}