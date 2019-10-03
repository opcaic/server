using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Games.Models
{
	public class GameReferenceDto : IMapFrom<Game>
	{
		public long Id { get; set; }

		public string Name { get; set; }
	}
}