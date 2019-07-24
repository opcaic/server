using Microsoft.Extensions.Logging;

namespace OPCAIC.Broker.Runner
{
	public class IdentityAwareLoggerFactory : ILoggerFactory
	{
		private readonly ILoggerFactory innerFactory;
		private readonly string identity;

		public IdentityAwareLoggerFactory(ILoggerFactory innerFactory, string identity)
		{
			this.innerFactory = innerFactory;
			this.identity = identity;
		}

		/// <inheritdoc />
		public void Dispose() => innerFactory.Dispose();

		/// <inheritdoc />
		public ILogger CreateLogger(string categoryName) => innerFactory.CreateLogger($"({identity}) - {categoryName}");

		/// <inheritdoc />
		public void AddProvider(ILoggerProvider provider) => innerFactory.AddProvider(provider);
	}
}