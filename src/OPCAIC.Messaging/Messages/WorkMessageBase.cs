using System;

namespace OPCAIC.Messaging.Messages
{
	[Serializable]
	public class WorkMessageBase
	{
		public string Game { get; set; }
		public int Id { get; set; }
	}
}