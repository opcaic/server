using OPCAIC.Domain.Enums;

namespace OPCAIC.Infrastructure.Dtos
{
	public class JobStateUpdateDto
	{
		public WorkerJobState State { get; set; }
	}
}