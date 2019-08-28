using System.Collections.Generic;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Dtos.Matches
{
	public class MatchDetailDto
	{
		public long Id { get; set; }
		public long Index { get; set; }
		public MatchState State { get; set; }
		public TournamentReferenceDto Tournament { get; set; }
		public IList<SubmissionReferenceDto> Submissions { get; set; }
		public IList<MatchExecutionDto> Executions { get; set; }
	}
}