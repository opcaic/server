using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class UpdateValidationStateDto
	{
		public UpdateValidationStateDto(SubmissionValidationState validationState)
		{
			ValidationState = validationState;
		}

		public SubmissionValidationState ValidationState { get; set; }
	}
}