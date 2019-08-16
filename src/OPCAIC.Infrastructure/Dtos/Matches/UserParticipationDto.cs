using System;
using System.Collections.Generic;
using System.Text;

namespace OPCAIC.Infrastructure.Dtos.Matches
{
	public class UserParticipationDto
	{
		public long MatchId { get; set; }
		public MatchReferenceDto Match { get; set; }
		public long UserId { get; set; }
		public UserReferenceDto User { get; set; }
	}
}
