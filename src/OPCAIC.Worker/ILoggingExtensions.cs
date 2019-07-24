using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Worker
{
	public static class ILoggingExtensions
	{
		public static IDisposable TaskScope(this ILogger logger, WorkMessageBase message)
		{
			return logger.BeginScope(new Dictionary<string, object>
			{
				[Constants.LogProperties.TaskId] = message.Id,
				[Constants.LogProperties.Game] = message.Game
			});
		}

		public static IDisposable EntryPointScope(this ILogger logger, string name)
		{
			return logger.BeginScope(new Dictionary<string, object>
			{
				[Constants.LogProperties.EntryPoint] = name
			});
		}
	}
}