using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Utils;

namespace OPCAIC.Worker.Services
{
	internal class SubmissionValidator
		: JobExecutorBase<SubmissionValidationRequest, SubmissionValidationResult>
	{
		/// <inheritdoc />
		public SubmissionValidator(ILogger<SubmissionValidator> logger, IExecutionServices services) :
			base(logger, services)
		{
		}

		/// <inheritdoc />
		protected override async Task InternalExecute()
		{
			Logger.LogInformation("Downloading submission");
			var srcDir = new DirectoryInfo(PathTo(Constants.DirectoryNames.Source));
            //var inDir = new DirectoryInfo(PathTo(Constants.DirectoryNames.Input));
            //var outDir = new DirectoryInfo(PathTo(Constants.DirectoryNames.Output));
            var binDir = Directory.CreateDirectory(PathTo(Constants.DirectoryNames.Binary));

            await Services.DownloadSubmission(Request.Path, srcDir.FullName);
			Require.That<InvalidOperationException>(srcDir.Exists, "Failed to download sources");

			Logger.LogInformation("Invoking checker");
			GameModule.Check(srcDir.FullName, WorkingDirectory.FullName);
			var filename = PathTo(Constants.FileNames.CheckerResult);
			Require.FileExists(filename, $"{Constants.FileNames.CheckerResult} was not produced");

			Logger.LogInformation("Invoking compiler");
			GameModule.Compile(srcDir.FullName, binDir.FullName, WorkingDirectory.FullName);
			filename = PathTo(Constants.FileNames.CompilerResult);
			Require.FileExists(filename, $"{Constants.FileNames.CompilerResult} was not produced");

			Logger.LogInformation("Invoking validator");
			GameModule.Validate(binDir.FullName, WorkingDirectory.FullName);
			filename = PathTo(Constants.FileNames.ValidatorResult);
			Require.FileExists(filename, $"{Constants.FileNames.ValidatorResult} was not produced");

			Result.Status = Status.Ok;
		}
	}
}
