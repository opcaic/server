using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace OPCAIC.TestUtils
{
	public class XUnitLoggerProvider : ILoggerProvider
	{
		private readonly ITestOutputHelper output;

		public XUnitLoggerProvider(ITestOutputHelper output) => this.output = output;

		/// <inheritdoc />
		public ILogger<T> CreateLogger<T>() => new XUnitLogger<T>(output);

		/// <inheritdoc />
		public ILogger CreateLogger(string categoryName) => new XUnitLogger(output, categoryName);

		/// <inheritdoc />
		public void Dispose()
		{
		}
	}
}
