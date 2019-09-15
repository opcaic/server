using System.Collections.Generic;
using OPCAIC.Infrastructure.Dtos.Submissions;

namespace OPCAIC.Infrastructure.Dtos.Matches
{
	public class NewMatchDto
	{
		public long TournamentId { get; set; }
		public long Index { get; set; }
		public List<long> Submissions { get; set; }
	}
}