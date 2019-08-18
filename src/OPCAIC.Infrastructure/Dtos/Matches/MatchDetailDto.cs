using System.Collections.Generic;
using OPCAIC.Infrastructure.Dtos.Documents;

namespace OPCAIC.Infrastructure.Dtos.Matches
{
	public class MatchDetailDto
	{
		public long Id { get; set; }
		public long Index { get; set; }
		public TournamentReferenceDto Tournament { get; set; }
		public IList<SubmissionParticipationDto> Participations { get; set; }
		public IList<MatchExecutionDto> Executions { get; set; }
	}
}