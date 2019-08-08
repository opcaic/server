using System.ComponentModel.DataAnnotations;
using OPCAIC.Infrastructure;

namespace OPCAIC.ApiService.Models.Games
{
	public class UpdateGameModel
	{
		[Required]
		[MinLength(1)]
		[MaxLength(StringLengths.GameName)]
		public string Name { get; set; }
	}
}