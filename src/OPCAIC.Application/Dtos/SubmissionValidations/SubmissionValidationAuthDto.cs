namespace OPCAIC.Application.Dtos.SubmissionValidations
{
	public class SubmissionValidationAuthDto
	{
		public long Id { get; set; }

		public long SubmissionAuthorId { get; set; }

		public long TournamentOwnerId { get; set; }

		public long[] TournamentManagersIds { get; set; }
	}
}