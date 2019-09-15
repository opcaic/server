using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Application.Dtos.SubmissionValidations;

namespace OPCAIC.Application.Interfaces
{
	public interface ILogStorageService
	{
		SubmissionValidationLogsDto GetSubmissionValidationLogs(
			SubmissionValidationStorageDto storage);

		MatchExecutionLogsDto GetMatchExecutionLogs(MatchExecutionStorageDto storage);
	}
}