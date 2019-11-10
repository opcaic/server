using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Moq;
using OPCAIC.ApiService.Services;
using OPCAIC.Application.Interfaces;
using OPCAIC.TestUtils;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Services
{
	public class LogStorageServiceTest : ServiceTestBase
	{
		/// <inheritdoc />
		public LogStorageServiceTest(ITestOutputHelper output) : base(output)
		{
			storageMock = Services.Mock<IStorageService>(MockBehavior.Strict);
		}

		private LogStorageService Service => GetService<LogStorageService>();

		private readonly Mock<IStorageService> storageMock;

		private MemoryStream CreateSubmissionValidationResult(string checker, string compiler, string validator)
		{
			var stream = new MemoryStream();
			using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
			{
				if (checker != null)
				{
					Output.WriteLine("Writing check.0.stdout");
					WriteEntry(archive, "check.0.stdout", checker);
				}
				if (compiler != null)
				{
					Output.WriteLine("Writing compile.0.stdout");
					WriteEntry(archive, "compile.0.stdout", compiler);
				}
				if (validator != null)
				{
					Output.WriteLine("Writing validate.0.stdout");
					WriteEntry(archive, "validate.0.stdout", validator);
				}
			}

			return stream;
		}

		private MemoryStream CreateMatchExecutionResult(IEnumerable<string> compiler, string executor)
		{
			var stream = new MemoryStream();
			using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
			{
				int order = 0;
				foreach (var cmp in compiler)
				{
					if (cmp != null)
					{
						Output.WriteLine($"Writing compile.{order}.stdout");
						WriteEntry(archive, $"compile.{order}.stdout", cmp);
					}

					order++;
				}

				if (executor != null)
				{
					Output.WriteLine($"Writing execute.stdout");
					WriteEntry(archive, "execute.stdout", executor);
				}
			}

			return stream;
		}

		private void WriteEntry(ZipArchive archive, string entryName, string content)
		{
			using (var stream = new StreamWriter(archive.CreateEntry(entryName).Open()))
			{
				stream.Write(content);
			}
		}

		[Theory]
		[InlineData("aaaa", "bbewf", "afwf")]
		[InlineData("ff", null, "afwf")]
		public void ReadsSubmissionLogs(string check, string compile, string validate)
		{
			storageMock.Setup(s => s.ReadSubmissionValidationResultArchive(null))
				.Returns(CreateSubmissionValidationResult(check, compile, validate));

			var logs = Service.GetSubmissionValidationLogs(null);

			logs.CheckerLog.ShouldBe(check);
			logs.CompilerLog.ShouldBe(compile);
			logs.ValidatorLog.ShouldBe(validate);
		}

		[Theory]
		[InlineData("aaaa", "bbewf", "afwf")]
		[InlineData("ff", null, "afwf")]
		public void ReadsMatchLogs(string execute, params string[] compile)
		{
			storageMock.Setup(s => s.ReadMatchResultArchive(null))
				.Returns(CreateMatchExecutionResult(compile, execute));

			var logs = Service.GetMatchExecutionLogs(null);

			logs.ExecutorLog.ShouldBe(execute);

			logs.SubmissionLogs.Count.ShouldBe(compile.Length);
			for (int i = 0; i < compile.Length; i++)
			{
				logs.SubmissionLogs[i].CompilerLog.ShouldBe(compile[i]);
			}
		}

		[Fact]
		public void NoMatchExecutionArchiveExists()
		{
			storageMock.Setup(s => s.ReadMatchResultArchive(null))
				.Returns((Stream)null);
			var logs = Service.GetMatchExecutionLogs(null);

			logs.ShouldNotBeNull();

			logs.ExecutorLog.ShouldBeNull();
			logs.SubmissionLogs.ShouldBeEmpty();
		}

		[Fact]
		public void NoSubmissionValidationArchiveExists()
		{
			storageMock.Setup(s => s.ReadSubmissionValidationResultArchive(null))
				.Returns((Stream)null);
			var logs = Service.GetSubmissionValidationLogs(null);

			logs.ShouldNotBeNull();

			logs.CheckerLog.ShouldBeNull();
			logs.CompilerLog.ShouldBeNull();
			logs.ValidatorLog.ShouldBeNull();
		}
	}
}