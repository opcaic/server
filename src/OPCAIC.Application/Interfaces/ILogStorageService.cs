using OPCAIC.Application.Dtos.BaseDtos;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Dtos.SubmissionValidations;

namespace OPCAIC.Application.Interfaces
{
	public interface ILogStorageService
	{
		SubmissionValidationLogsDto GetSubmissionValidationLogs(
			SubmissionValidationDtoBase storage);

		MatchExecutionLogsDto GetMatchExecutionLogs(MatchExecutionDtoBase storage);
	}
}