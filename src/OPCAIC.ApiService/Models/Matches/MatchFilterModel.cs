using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Models.Matches
{
	public class MatchFilterModel : FilterModelBase
	{
		public long? TournamentId { get; set; }
		public long? UserId { get; set; }
		public bool? Executed { get; set; }
	}
}
