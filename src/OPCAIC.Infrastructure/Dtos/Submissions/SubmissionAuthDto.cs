﻿namespace OPCAIC.Infrastructure.Dtos.Submissions
{
	public class SubmissionAuthDto
	{
		public long AuthorId { get; set; }
		public long TournamentOwnerId { get; set; }
		public long[] TournamentManagersIds { get; set; }
	}
}