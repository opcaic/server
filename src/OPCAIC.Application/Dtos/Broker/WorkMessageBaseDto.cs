using System;

namespace OPCAIC.Infrastructure.Dtos.Broker
{
	public class WorkMessageBaseDto
	{
		public Guid JobId { get; set; }
		public string Game { get; set; }
	}
}