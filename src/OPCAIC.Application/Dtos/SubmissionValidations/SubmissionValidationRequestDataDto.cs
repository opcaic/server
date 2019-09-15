namespace OPCAIC.Application.Dtos.SubmissionValidations
{
	public class SubmissionValidationRequestDataDto
		: JobDtoBase
	{
		public long Id { get; set; }
		public long SubmissionId { get; set; }
	}
}