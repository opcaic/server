using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Submissions.Models;
using OPCAIC.Application.SubmissionValidations.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class SubmissionDetailDto : SubmissionPreviewDtoBase, IMapFrom<Submission>
	{
		public SubmissionValidationDetailDto LastValidation { get; set; }
	}
}