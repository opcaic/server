﻿using System;
using System.Collections.Generic;

namespace OPCAIC.Messaging.Messages
{
	[Serializable]
	public class ExecuteMatchMessage
	{
		public string Game { get; set; }

		public List<BotInfo> Bots { get; set; }
	}
}