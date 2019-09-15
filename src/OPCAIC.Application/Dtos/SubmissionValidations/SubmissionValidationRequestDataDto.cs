using OPCAIC.Infrastructure.Dtos.Tournaments;

namespace OPCAIC.Infrastructure.Dtos.SubmissionValidations
{
	public class SubmissionValidationRequestDataDto
		: JobDtoBase
	{
		public long Id { get; set; }
		public long SubmissionId { get; set; }
	}
}