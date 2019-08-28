using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.Services;

namespace OPCAIC.Broker.Runner
{
	public class DummyDownloadServiceFactory : IDownloadServiceFactory
	{
		private readonly ILoggerFactory loggerFactory;
		private readonly IOptions<FileServerConfig> config;

		public DummyDownloadServiceFactory(ILoggerFactory loggerFactory, IOptions<FileServerConfig> config)
		{
			this.loggerFactory = loggerFactory;
			this.config = config;
		}

		/// <inheritdoc />
		public IDownloadService Create(string accessToken)
		{
			return new DummyDownloadService(loggerFactory.CreateLogger<DummyDownloadService>(), config);
		}
	}
}