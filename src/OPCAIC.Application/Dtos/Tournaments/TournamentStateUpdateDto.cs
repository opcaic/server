using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Tournaments
{
	public class TournamentStateUpdateDto : IMapTo<Tournament>
	{
		public TournamentState State { get; set; }
	}
}