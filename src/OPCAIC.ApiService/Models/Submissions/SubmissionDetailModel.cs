using System.Collections.Generic;
using OPCAIC.ApiService.Models.SubmissionValidations;
using OPCAIC.Application.Submissions.Models;

namespace OPCAIC.ApiService.Models.Submissions
{
	public class SubmissionDetailModel : SubmissionPreviewDto
	{
		public List<SubmissionValidationPreviewModel> Validations { get; set; }
	}
}