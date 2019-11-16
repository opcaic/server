using System;
using System.Collections.Generic;
using System.Linq;
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

		public static IDisposable SimpleScope<T>(this ILogger logger, string name, T value)
		{
			return logger.BeginScope(new Dictionary<string, object>
			{
				[name] = value
			});
		}

		public static IDisposable SimpleScope(this ILogger logger, params (string key, object value)[] values)
		{
			return logger.BeginScope(values.ToDictionary(v => v.key, v => v.value));
		}
	}
}