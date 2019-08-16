using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Models.Matches
{
	public class UserParticipationModel
	{
		public long MatchId { get; set; }
		public MatchReferenceModel Match { get; set; }
		public long UserId { get; set; }
		public UserReferenceModel User { get; set; }
	}
}
