using Microsoft.AspNetCore.Http;

namespace OPCAIC.ApiService.Models.Submissions
{
	public class NewSubmissionModel
	{
		public long TournamentId { get; set; }

		public IFormFile Archive { get; set; }
	}
}