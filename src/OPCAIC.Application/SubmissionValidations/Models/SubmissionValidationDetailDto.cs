using AutoMapper;
using OPCAIC.Application.Dtos.SubmissionValidations;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.SubmissionValidations.Models
{
	public class SubmissionValidationDetailDto : SubmissionValidationPreviewDto, IMapFrom<SubmissionValidation>
	{
		[IgnoreMap]
		public string CheckerLog { get; set; }

		[IgnoreMap]
		public string CompilerLog { get; set; }

		[IgnoreMap]
		public string ValidatorLog { get; set; }

		public virtual void AddLogs(SubmissionValidationLogsDto logs)
		{
			CheckerLog = logs.CheckerLog;
			CompilerLog = logs.CompilerLog;
			ValidatorLog = logs.ValidatorLog;
		}
	}
}