using System.Collections.Generic;
using OPCAIC.ApiService.Models.SubmissionValidations;

namespace OPCAIC.ApiService.Models.Submissions
{
	public class SubmissionDetailModel : SubmissionPreviewModel
	{
		public List<SubmissionValidationPreviewModel> Validations { get; set; }
	}
}