using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace OPCAIC.TestUtils
{
	public class XUnitLoggerFactory
	{
		private readonly ITestOutputHelper output;

		public XUnitLoggerFactory(ITestOutputHelper output) => this.output = output;

		/// <inheritdoc />
		public ILogger<T> CreateLogger<T>() => new XUnitLogger<T>(output);
	}
}
