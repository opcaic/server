using System.Collections.Generic;
using AutoMapper;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Tournaments.Models
{
	public class TournamentDetailDto : TournamentDtoBase, IMapFrom<Tournament>
	{
		public bool PrivateMatchLog { get; set; }

		public string Description { get; set; }

		public long MaxSubmissionSize { get; set; }

		[IgnoreMap] // has to be mapped in-memory manually due to inheritance
		public List<MenuItemDto> MenuItems { get; set; }
	}
}