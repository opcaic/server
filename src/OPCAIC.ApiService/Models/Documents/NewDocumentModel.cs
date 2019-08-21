using System.ComponentModel.DataAnnotations;
using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using OPCAIC.Infrastructure;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.ApiService.Models.Documents
{
	public class NewDocumentModel
	{
		[ApiRequired]
		[ApiMaxLength(StringLengths.DocumentName)]
		public string Name { get; set; }

		public string Content { get; set; }

		[ApiEntityReference(typeof(Tournament))]
		public long TournamentId { get; set; }
	}
}