using System.Collections.Generic;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Submissions.Models;
using OPCAIC.Application.SubmissionValidations.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class SubmissionAdminDto : SubmissionPreviewDtoBase, IMapFrom<Submission>
	{
		public List<SubmissionValidationAdminDto> Validations { get; set; }
	}
}