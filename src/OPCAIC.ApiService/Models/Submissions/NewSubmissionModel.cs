using Microsoft.AspNetCore.Http;
using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.ApiService.Models.Submissions
{
	public class NewSubmissionModel
	{
		[ApiRequired]
		[ApiEntityReference(typeof(Tournament))]
		public long TournamentId { get; set; }

		[ApiRequired]
		public IFormFile Archive { get; set; }
	}
}