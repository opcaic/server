using System;

namespace OPCAIC.Infrastructure.Dtos.Broker
{
	public class WorkerInfoDto
	{
		public string Identity { get; set; }
		public Guid? CurrentJob { get; set; }
	}
}