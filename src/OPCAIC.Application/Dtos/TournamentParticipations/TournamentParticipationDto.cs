using OPCAIC.Application.Dtos.Submissions;

namespace OPCAIC.Application.Dtos.TournamentParticipations
{
	public class UpdateTournamentParticipationDto
	{
		public UpdateTournamentParticipationDto(long activeSubmissionId)
		{
			ActiveSubmissionId = activeSubmissionId;
		}

		public long ActiveSubmissionId { get; }
	}
}