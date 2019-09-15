using System;

namespace OPCAIC.Application.Dtos.Broker
{
	public class WorkMessageBaseDto
	{
		public Guid JobId { get; set; }
		public string Game { get; set; }
	}
}