using System;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.ApiService.Models.Submissions
{
	public class SubmissionPreviewModel
	{
		public long Id { get; set; }
		public double Score { get; set; }
		public UserReferenceModel Author { get; set; }
		public TournamentReferenceModel Tournament { get; set; }
		public bool IsActive { get; set; }
		public DateTime Created { get; set; }
		public SubmissionValidationState ValidationState { get; set; }
	}
}