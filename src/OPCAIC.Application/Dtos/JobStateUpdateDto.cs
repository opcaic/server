using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos
{
	public class JobStateUpdateDto
	{
		public WorkerJobState State { get; set; }
	}
}