using System;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Tournaments
{
	public class TournamentStateInfoDto : IMapFrom<Tournament>
	{
		public long Id { get; set; }
		public TournamentState State { get; set; }
		public TournamentScope Scope { get; set; }
		public DateTime? Deadline { get; set; }
	}
}