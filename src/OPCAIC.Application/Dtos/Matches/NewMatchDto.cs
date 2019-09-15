using System.Collections.Generic;

namespace OPCAIC.Application.Dtos.Matches
{
	public class NewMatchDto
	{
		public long TournamentId { get; set; }
		public long Index { get; set; }
		public List<long> Submissions { get; set; }
	}
}