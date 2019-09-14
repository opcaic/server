using System;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Dtos.Users;

namespace OPCAIC.Infrastructure.Dtos.Submissions
{
	public class SubmissionPreviewDto
	{
		public long Id { get; set; }
		public double Score { get; set; }
		public UserReferenceDto Author { get; set; }
		public TournamentReferenceDto Tournament { get; set; }
		public bool IsActive { get; set; }
		public DateTime Created { get; set; }
		public SubmissionValidationDto LastValidation { get; set; }
	}
}