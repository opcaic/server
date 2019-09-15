using System;

namespace OPCAIC.Infrastructure.Dtos.SubmissionValidations
{
	public class NewSubmissionValidationDto
	{
		public long SubmissionId { get; set; }
		public Guid JobId { get; set; }
	}
}