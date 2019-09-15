using System;

namespace OPCAIC.Application.Dtos.SubmissionValidations
{
	public class NewSubmissionValidationDto
	{
		public long SubmissionId { get; set; }
		public Guid JobId { get; set; }
	}
}