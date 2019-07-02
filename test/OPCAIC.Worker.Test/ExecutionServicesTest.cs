using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using OPCAIC.Messaging.Messages;
using OPCAIC.TestUtils;
using OPCAIC.Worker.Exceptions;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	public class ExecutionServicesTest : ServiceTestBase, IDisposable
	{
		private readonly Mock<IDownloadService> downloadService;
		private readonly Mock<IGameModuleRegistry> gameModuleRegistry;
		private readonly DirectoryInfo workdir;
		private ExecutionServices ExecutionServices => GetService<ExecutionServices>();

		/// <inheritdoc />
		public ExecutionServicesTest(ITestOutputHelper output) : base(output)
		{
			workdir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

			Services.AddSingleton(Options.Create(new ExecutionConfig()
			{
				WorkingDirectoryRoot = workdir.FullName
			}));
			downloadService = Services.Mock<IDownloadService>();
			gameModuleRegistry = Services.Mock<IGameModuleRegistry>();
		}

		[Fact]
		public void CreatesWorkDirForJob()
		{
			var id = new Random().Next();

			var dir = ExecutionServices.GetWorkingDirectory(new MatchExecutionRequest() {Id = id});

			Assert.True(dir.Exists);
			Assert.True(Directory.Exists(Path.Combine(workdir.FullName, id.ToString())));
		}

		[Fact]
		public void ThrowsWhenGameModuleDoesNotExist()
		{
			gameModuleRegistry.Setup(s => s.FindGameModule(It.IsAny<string>())).Returns((IGameModule)null);

			Assert.Throws<GameModulueNotFoundException>(() => ExecutionServices.GetGameModule("game"));
		}

		[Fact]
		public async Task SuccessfullyDownloadsSubmission()
		{
			const string fileContents = "ABCD";
			const string fileName = "a";
			var memstream = new MemoryStream();

			using (var archive = new ZipArchive(memstream, ZipArchiveMode.Create))
			using (var stream = new StreamWriter(archive.CreateEntry(fileName).Open()))
			{
				stream.Write(fileContents);
			}

			downloadService.Setup(s => s.DownloadBinaryAsync(It.IsAny<string>()))
				.ReturnsAsync(memstream.ToArray());
			await ExecutionServices.DownloadSubmission("ignored", workdir.FullName);

			var file = workdir.GetFiles().SingleOrDefault();
			Assert.NotNull(file);
			Assert.Equal(fileName, file.Name);
			using (var stream = file.OpenText())
			{
				Assert.Equal(fileContents, stream.ReadToEnd());
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			workdir.Delete(true);
		}
	}
}