using System;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Dtos.Tournaments
{
	public class TournamentStateInfoDto
	{
		public long Id { get; set; }
		public TournamentState State { get; set; }
		public TournamentScope Scope { get; set; }
		public DateTime? Deadline { get; set; }
	}
}