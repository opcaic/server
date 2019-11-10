using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.SubmissionValidations.Models;
using OPCAIC.Application.SubmissionValidations.Queries;

namespace OPCAIC.Application.Dtos.SubmissionValidations
{
	public class SubmissionValidationLogsDto : IMapTo<SubmissionValidationAdminDto>
	{
		public string CheckerLog { get; set; }
		public string CheckerErrorLog { get; set; }
		public string CompilerLog { get; set; }
		public string CompilerErrorLog { get; set; }
		public string ValidatorLog { get; set; }
		public string ValidatorErrorLog { get; set; }
	}
}