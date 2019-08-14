using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace OPCAIC.TestUtils
{
	public class XUnitLoggerProvider : ILoggerProvider
	{
		private readonly ITestOutputHelper output;

		public XUnitLoggerProvider(ITestOutputHelper output)
		{
			this.output = output;
		}

		/// <inheritdoc />
		public ILogger CreateLogger(string categoryName)
		{
			return new XUnitLogger(output, categoryName);
		}

		/// <inheritdoc />
		public void Dispose()
		{
		}

		/// <inheritdoc />
		public ILogger<T> CreateLogger<T>()
		{
			return new XUnitLogger<T>(output);
		}
	}
}