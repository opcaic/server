using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Services.Test
{
	public class StorageServiceTest : ServiceTestBase
	{
		/// <inheritdoc />
		public StorageServiceTest(ITestOutputHelper output) : base(output)
		{
			Services.Configure<StorageConfiguration>(cfg =>
			{
				cfg.Directory = NewDirectory().FullName;
			});
		}

		private StorageService StorageService => GetService<StorageService>();

		[Fact]
		public void CorrectlySavesFile()
		{
			var sub = new SubmissionStorageDto {Id = 1};
			var contents = new byte[] {1, 2, 3, 4, 5, 6, 7, 8};
			using (var s = StorageService.WriteSubmissionArchive(sub))
			{
				s.Write(contents, 0, contents.Length);
			}

			using (var s = StorageService.ReadSubmissionArchive(sub))
			{
				var buffer = new byte[1024];
				var read = s.Read(buffer, 0, buffer.Length);

				Assert.Equal(contents.Length, read);
				Assert.Equal(contents, buffer.Take(read));
			}
		}

		[Fact]
		public void ThrowsWhenFileAlreadyExists()
		{
			// create file first
			var sub = new SubmissionStorageDto {Id = 1};
			using (var s = StorageService.WriteSubmissionArchive(sub))
			{
				s.WriteByte(1);
			}

			// try overwrite
			Assert.Throws<InvalidOperationException>(
				() => StorageService.WriteSubmissionArchive(sub));
		}
	}
}