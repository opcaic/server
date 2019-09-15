using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.MatchExecutions
{
	public class NewSubmissionMatchResultDto
	{
		public long SubmissionId { get; set; }
		public double Score { get; set; }
		public EntryPointResult CompilerResult { get; set; }
		public bool Crashed { get; set; }
		public string AdditionalData { get; set; }
	}
}