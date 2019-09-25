using System.Collections.Generic;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Submissions.Models;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class SubmissionDetailDto : SubmissionPreviewDto, IMapFrom<Submission>
	{
		public List<SubmissionValidationDto> Validations { get; set; }
	}
}