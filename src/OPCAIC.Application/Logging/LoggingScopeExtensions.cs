using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace OPCAIC.Application.Logging
{
	public static class LoggingScopeExtensions
	{
		public static IDisposable CreateScopeWithIds(this ILogger logger, object obj)
		{
			var ids = new Dictionary<string, object>();

			foreach (var propertyInfo in obj.GetType().GetProperties())
			{
				if (propertyInfo.Name.EndsWith("id", StringComparison.OrdinalIgnoreCase))
				{
					ids.Add(propertyInfo.Name, propertyInfo.GetValue(obj));
				}
			}

			return logger.BeginScope(ids);
		}
	}
}