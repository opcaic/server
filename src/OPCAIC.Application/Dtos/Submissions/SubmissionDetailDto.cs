using System.Collections.Generic;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Submissions.Models;
using OPCAIC.Application.SubmissionValidations.Models;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class SubmissionDetailDto : SubmissionPreviewDto, IMapFrom<Submission>
	{
		public List<SubmissionValidationPreviewDto> Validations { get; set; }
	}
}