using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.Models.Games
{
	public class GameFilterModel : FilterModelBase
	{
		[MinLength(1)]
		public string Name { get; set; }
	}
}