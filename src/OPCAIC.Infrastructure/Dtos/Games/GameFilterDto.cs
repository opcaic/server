using System.ComponentModel.DataAnnotations;

namespace OPCAIC.Infrastructure.Dtos.Games
{
	public class GameFilterDto : FilterDtoBase
	{
		public const string SortByName = "name";
		public const string SortByCreated = "created";

		public string Name { get; set; }
	}
}