using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OPCAIC.Application.Emails;
using OPCAIC.Worker;
using OPCAIC.Worker.Config;
using Xunit;

namespace OPCAIC.FunctionalTest
{
	public class FunctionalTestFixture : IDisposable
	{
		private readonly DirectoryInfo tempWorkDir = Directory.CreateDirectory(Path.GetRandomFileName());
		private readonly DirectoryInfo tempArchiveDir = Directory.CreateDirectory(Path.GetRandomFileName());

		public FunctionalTestFixture()
		{
			WorkerHost = Program.CreateHostBuilder(Array.Empty<string>())
			.ConfigureServices(services =>
			{
				services.Configure<ExecutionConfig>(cfg =>
				{
					cfg.ArchiveDirectoryRoot = tempArchiveDir.FullName + '/';
					cfg.WorkingDirectoryRoot = tempWorkDir.FullName + '/';
				});
			}).Start();

			ClientFactory.AddServiceOverride<IEmailService>(EmailService);

			// make sure the server is already initialized
			ClientFactory.CreateClient();
		}

		public readonly IHost WorkerHost;

		public WebServerFactory ClientFactory { get; } = new WebServerFactory();

		public NullEmailService EmailService { get; } = new NullEmailService();

		public TService GetServerService<TService>()
		{
			return ClientFactory.Server.Host.Services.GetRequiredService<TService>();
		}

		/// <inheritdoc />
		public void Dispose()
		{
			WorkerHost.StopAsync().GetAwaiter().GetResult();
			WorkerHost?.Dispose();
			ClientFactory?.Dispose();

			tempWorkDir.Delete(true);
			tempArchiveDir.Delete(true);
		}
	}

	[CollectionDefinition("ServerContext")]
	public class FunctionalTestContextCollection : ICollectionFixture<FunctionalTestFixture>
	{
		// no code inside needed, this class is never instantiated, see xUnit documentation for
		// Collection fixtures
	}
}