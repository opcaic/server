using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.TestUtils;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.Services;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	public class DownloadServiceTest : ServiceTestBase
	{
		/// <inheritdoc />
		public DownloadServiceTest(ITestOutputHelper output) : base(output)
		{
			fileServerMockDir = NewDirectory();
			fileServerMockDir.CreateSubdirectory(SubmissionDir);
			fileServerMockDir.CreateSubdirectory(ResultsDir);
			// setup
			Services.Configure<FileServerConfig>(cfg =>
			{
				// HACK: WebClient is able to fetch files from local folder
				cfg.ServerAddress = fileServerMockDir.FullName + '\\';
			});
			downloadService = GetService<DownloadService>();
		}

		private readonly DownloadService downloadService;

		// directory structure needs to match the uri in order to work properly
		private readonly DirectoryInfo fileServerMockDir;

		private const string FileContents = "ABCD";
		private const string FileName = "a";
		private const string SubmissionDir = "submissions";
		private const string ResultsDir = "results";
		private const long SubmissionId = 1;

		private void CreateMockArchive(string path)
		{
			using (var fileStream = File.Open(path, FileMode.CreateNew))
			using (var archive = new ZipArchive(fileStream, ZipArchiveMode.Create))
			using (var stream = new StreamWriter(archive.CreateEntry(FileName).Open()))
			{
				stream.Write(FileContents);
			}
		}

		private void CreateMockFile(string path)
		{
			using (var stream = new StreamWriter(path))
			{
				stream.Write(FileContents);
			}
		}

		private void CheckDirectoryForMockFile(DirectoryInfo directory)
		{
			var file = directory.GetFiles().SingleOrDefault();
			Assert.NotNull(file);
			Assert.Equal(FileName, file.Name);
			using (var stream = file.OpenText())
			{
				Assert.Equal(FileContents, stream.ReadToEnd());
			}
		}

		[Fact]
		public async Task SuccessfullyDownloadsSubmission()
		{
			CreateMockArchive(Path.Combine(fileServerMockDir.FullName, SubmissionDir,
				SubmissionId.ToString()));

			var targetDir = NewDirectory();
			await downloadService.DownloadSubmission(SubmissionId, targetDir.FullName);

			CheckDirectoryForMockFile(targetDir);
		}

		[Fact]
		public async Task SuccessfullyUploadsResults()
		{
			var srcDir = NewDirectory();
			CreateMockFile(Path.Combine(srcDir.FullName, FileName));

			await downloadService.UploadValidationResults(1, srcDir.FullName);

			var tmpDir = NewDirectory();
			ZipFile.ExtractToDirectory(
				Path.Combine(fileServerMockDir.FullName, ResultsDir, SubmissionId.ToString()),
				tmpDir.FullName);

			CheckDirectoryForMockFile(tmpDir);
		}
	}
}