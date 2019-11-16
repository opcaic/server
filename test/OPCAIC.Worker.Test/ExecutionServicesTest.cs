using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using OPCAIC.GameModules.Interface;
using OPCAIC.Messaging.Messages;
using OPCAIC.TestUtils;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.Exceptions;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	public class ExecutionServicesTest : ServiceTestBase
	{
		/// <inheritdoc />
		public ExecutionServicesTest(ITestOutputHelper output) : base(output)
		{
			workdir = NewDirectory();

			Services.AddSingleton(Options.Create(new ExecutionConfig
			{
				WorkingDirectory = workdir.FullName,
				ErrorDirectory = NewDirectory().FullName,
				ArchiveDirectory = NewDirectory().FullName
			}));
			Services.Mock<IDownloadService>();
			gameModuleRegistry = Services.Mock<IGameModuleRegistry>();
		}

		private readonly Mock<IGameModuleRegistry> gameModuleRegistry;
		private readonly DirectoryInfo workdir;
		private ExecutionServices ExecutionServices => GetService<ExecutionServices>();

		[Fact]
		public void CreatesWorkDirForJob()
		{
			var id = Guid.NewGuid().ToString();

			var dir = ExecutionServices.GetWorkingDirectory(id);

			Assert.True(dir.Exists);
			Assert.True(Directory.Exists(Path.Combine(workdir.FullName, id)));
		}

		[Fact]
		public void ThrowsWhenGameModuleDoesNotExist()
		{
			gameModuleRegistry.Setup(s => s.FindGameModule(It.IsAny<string>()))
				.Returns((IGameModule)null);

			Assert.Throws<GameModuleNotFoundException>(
				() => ExecutionServices.GetGameModule("game"));
		}

		[Fact]
		public async Task DeletesOldFiles()
		{
			var dir = NewDirectory();
			var fileA = new FileInfo(Path.Combine(dir.FullName, "aaa"));
			using (var writer = fileA.CreateText())
			{
				writer.WriteLine("awefawefiawefoawiefawoefiaweofawe");
			}

			// make sure the timestamps will be different
			await Task.Delay(50);

			var fileB = new FileInfo(Path.Combine(dir.FullName, "bbb"));
			using (var writer = fileB.CreateText())
			{
				writer.WriteLine("awefawefiawefoawiefawoefiaweofawe");
			}

			dir.GetFileSystemInfos().Length.ShouldBe(2);

			// take timestamp in the middle
			var timestamp = fileA.CreationTime + (fileB.CreationTime - fileA.CreationTime) / 2;

			ExecutionServices.DeleteOldFilesInDirectory(dir, timestamp);

			dir.GetFileSystemInfos().Length.ShouldBe(1);
			File.Exists(fileA.FullName).ShouldBeFalse();
			File.Exists(fileB.FullName).ShouldBeTrue();
		}
	}
}