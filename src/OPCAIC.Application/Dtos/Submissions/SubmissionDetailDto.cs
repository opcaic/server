using System.Collections.Generic;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Submissions.Models;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class SubmissionDetailDto : SubmissionPreviewDto
	{
		public List<SubmissionValidationDto> Validations { get; set; }
	}
}