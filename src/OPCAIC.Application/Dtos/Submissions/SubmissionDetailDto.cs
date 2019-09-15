using System.Collections.Generic;
using OPCAIC.Application.Dtos.SubmissionValidations;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class SubmissionDetailDto : SubmissionPreviewDto
	{
		public List<SubmissionValidationDto> Validations { get; set; }
	}
}