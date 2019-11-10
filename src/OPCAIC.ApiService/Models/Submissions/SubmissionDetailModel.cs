using System.Collections.Generic;
using OPCAIC.Application.Submissions.Models;
using OPCAIC.Application.SubmissionValidations.Models;

namespace OPCAIC.ApiService.Models.Submissions
{
	public class SubmissionDetailModel : SubmissionPreviewDto
	{
		public List<SubmissionValidationPreviewDto> Validations { get; set; }
	}
}