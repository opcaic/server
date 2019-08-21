using Microsoft.AspNetCore.Http;
using OPCAIC.ApiService.Models.Documents;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.ApiService.Models.Submission
{
	public class NewSubmissionModel
	{
		[ApiRequired]
		[ApiEntityReference(typeof(Tournament))]
		public long TournamentId { get; set; }

		[ApiRequired]
		public IFormFile Archive { get; set; }
	}

	public class SubmissionFilterModel : FilterModelBase
	{
		public long? AuthorId { get; set; }
		public bool? IsActive { get; set; }
		public long? TournamentId { get; set; }
		public long? MatchId { get; set; }
	}

	public class SubmissionPreviewModel
	{
		public long Id { get; set; }
		public UserReferenceModel Author { get; set; }
		public TournamentReferenceModel Tournament { get; set; }
	}

	public class SubmissionDetailModel : SubmissionPreviewModel
	{
		public bool IsActive { get; set; }
	}

	public class SubmissionReferenceModel
	{
		public long Id { get; set; }
	}

	public class UpdateSubmissionModel
	{
		public bool IsActive { get; set; }
	}
}