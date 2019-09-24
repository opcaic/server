using System;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Submissions.Models
{
	public class SubmissionPreviewDto
	{
		public long Id { get; set; }
		public double Score { get; set; }
		public UserReferenceDto Author { get; set; }
		public TournamentReferenceDto Tournament { get; set; }
		public bool IsActive { get; set; }
		public DateTime Created { get; set; }
		public SubmissionValidationState ValidationState { get; set; }
	}
}