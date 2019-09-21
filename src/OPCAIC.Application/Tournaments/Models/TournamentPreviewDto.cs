using System;

namespace OPCAIC.Application.Tournaments.Models
{
	public class TournamentPreviewDto : TournamentDtoBase
	{
		public DateTime? LastUserSubmissionDate { get; set; }
	}
}