using System.Linq;
using OPCAIC.Infrastructure.Dtos.MatchExecutions;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;

namespace OPCAIC.ApiService.Services
{
	public interface ILogStorageService
	{
		SubmissionValidationLogsDto GetSubmissionValidationLogs(
			SubmissionValidationStorageDto storage);

		MatchExecutionLogsDto GetMatchExecutionLogs(MatchExecutionStorageDto storage);
	}
}