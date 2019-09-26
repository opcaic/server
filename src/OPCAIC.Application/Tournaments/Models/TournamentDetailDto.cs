using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Tournaments.Models
{
	public class TournamentDetailDto : TournamentDtoBase, IMapFrom<Tournament>
	{
		public bool PrivateMatchlog { get; set; }

		public string Description { get; set; }

		public string MenuData { get; set; }
	}
}