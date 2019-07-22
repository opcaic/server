using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using OPCAIC.Worker.Exceptions;

namespace OPCAIC.Worker.GameModules
{
	/// <summary>
	/// Represents explicitly results of the process. 
	/// </summary>
	enum ProcessResult
	{
		Failed, Succeeded, Killed
	}

	/// <summary>
	/// Provides method for executing a file in an asynchronous process.
	/// </summary>
	class ProcessHandler
	{
		/// <summary>
		/// Executes a file in an asynchronous process.
		/// </summary>
		/// <param name="rootDir">Working directory of the process.</param>
		/// <param name="fileName">Full name of the file to be executed</param>
		/// <param name="args">Arguments of the process.</param>
		/// <returns>Task with process' resulting exitcode.</returns>
		public static async Task<ProcessResult> RunProcessAsync(string rootDir, string fileName,
			string args)
		{
			using (var process = new Process
			{
				StartInfo =
				{
					FileName = fileName,
					Arguments = args,
					UseShellExecute = false,
					CreateNoWindow = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					WorkingDirectory = rootDir
				},
				EnableRaisingEvents = true
			})
			{
				bool started = process.Start();

				/*
				process.OutputDataReceived += (object sender, DataReceivedEventArgs e) => Console.WriteLine("result >>" + e.Data);
				process.BeginOutputReadLine();

				process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) => Console.WriteLine("error >>" + e.Data);
				process.BeginErrorReadLine();*/

				if (!started)
				{
					throw new ProcessStartException(fileName);
				}

				await Task.Yield();

				var task = Task.Run(() => process.WaitForExit());
				await task;

				return GetProcessResult(process);
			}
		}

		/// <summary>
		/// Based on process' exitcode, returns a ProcessResult.
		/// </summary>
		/// <param name="process">Process to get the result from.</param>
		/// <returns>Explicit result of the process.</returns>
		private static ProcessResult GetProcessResult(Process process)
		{
			switch (process.ExitCode)
			{
				case -1:
					return ProcessResult.Killed;
				case 0:
					return ProcessResult.Succeeded;
				default:
					return ProcessResult.Failed;
			}
		}
	}
}