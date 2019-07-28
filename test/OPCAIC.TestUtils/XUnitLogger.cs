using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace OPCAIC.TestUtils
{
	public class XUnitLogger : ILogger
	{
		private readonly string categoryName;
		private readonly ILogger nested;
		private readonly ITestOutputHelper output;

		public XUnitLogger(ITestOutputHelper output, string categoryName, ILogger nested = null)
		{
			this.output = output;
			this.categoryName = categoryName;
			this.nested = nested;
		}

		/// <inheritdoc />
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
			Func<TState, Exception, string> formatter)
		{
			output.WriteLine(
				$"{LevelToString(logLevel)}: [{Thread.CurrentThread.ManagedThreadId}] {DateTime.Now.TimeOfDay} - {categoryName}\n" +
				$"      {formatter(state, exception)}" + (exception != null ? "\n" +
				$"      {exception}": ""));

			nested?.Log(logLevel, eventId, state, exception, formatter);
		}

		/// <inheritdoc />
		public bool IsEnabled(LogLevel logLevel) => true;

		/// <inheritdoc />
		public IDisposable BeginScope<TState>(TState state) => new Scope();

		private static string LevelToString(LogLevel level)
		{
			switch (level)
			{
				case LogLevel.Trace:
					return "TRAC";
				case LogLevel.Debug:
					return "DBUG";
				case LogLevel.Information:
					return "INFO";
				case LogLevel.Warning:
					return "WARN";
				case LogLevel.Error:
					return "ERRO";
				case LogLevel.Critical:
					return "FAIL";
				case LogLevel.None:
					return "NONE";
				default:
					throw new ArgumentOutOfRangeException(nameof(level), level, null);
			}
		}

		private class Scope : IDisposable
		{
			/// <inheritdoc />
			public void Dispose()
			{
			}
		}
	}

	public class XUnitLogger<T> : XUnitLogger, ILogger<T>
	{
		public XUnitLogger(ITestOutputHelper output, ILogger nested = null)
			: base(output, typeof(T).Name, nested)
		{
		}
	}
}
