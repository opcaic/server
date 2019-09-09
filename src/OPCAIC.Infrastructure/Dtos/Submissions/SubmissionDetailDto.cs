using System;
using System.Collections.Generic;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;

namespace OPCAIC.Infrastructure.Dtos.Submissions
{
	public class SubmissionDetailDto : SubmissionPreviewDto
	{
		public List<SubmissionValidationDto> Validations { get; set; }
	}
}