using System;
using System.Collections.Generic;
using System.Text;

namespace OPCAIC.Infrastructure.Dtos.Matches
{
	public class MatchDetailDto
	{
		public long Id { get; set; }
		public long Index { get; set; }
		public TournamentReferenceDto Tournament { get; set; }
		public IList<UserReferenceDto> Participators { get; set; }
		public IList<SubmissionMatchResultReferenceDto> Results { get; set; }
	}
}
