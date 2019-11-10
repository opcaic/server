using System;

namespace OPCAIC.Application.Dtos.Broker
{
	public class WorkMessageBaseDto
	{
		public Guid JobId { get; set; }
		public string GameKey { get; set; }
	}
}