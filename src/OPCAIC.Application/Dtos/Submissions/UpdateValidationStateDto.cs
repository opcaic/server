using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class UpdateValidationStateDto : IMapTo<Submission>
	{
		public UpdateValidationStateDto(SubmissionValidationState validationState)
		{
			ValidationState = validationState;
		}

		public SubmissionValidationState ValidationState { get; set; }
	}
}