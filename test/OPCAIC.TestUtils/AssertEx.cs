using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Xunit;
using Xunit.Sdk;

namespace OPCAIC.TestUtils
{
	public static class AssertEx
	{
		public static void WaitForEvent(ManualResetEventSlim handle, TimeSpan timeout)
		{
			Debug.Assert(handle != null);

			if (Debugger.IsAttached)
			{
				timeout = TimeSpan.FromMilliseconds(1000 * 60 * 60);
			}

			if (!handle.Wait(timeout))
			{
				throw new XunitException("The event was not signaled in given time");
			}
		}

		public static void WaitForEvent(ManualResetEventSlim handle, double milliseconds)
		{
			Debug.Assert(milliseconds >= 0);

			WaitForEvent(handle, TimeSpan.FromMilliseconds(milliseconds));
		}

		public static void WaitForEvent(Func<bool> condition, TimeSpan timeout)
		{
			Debug.Assert(condition != null);

			if (Debugger.IsAttached)
			{
				timeout = TimeSpan.FromMilliseconds(1000 * 60 * 60);
			}

			var sw = Stopwatch.StartNew();
			while (!condition())
			{
				// wait
			}

			sw.Stop();
			if (sw.Elapsed > timeout)
			{
				throw new XunitException("The event was not signaled in given time");
			}
		}

		public static void FileExists(string path)
		{
			Debug.Assert(path != null);

			Assert.True(File.Exists(path), $"File '{path}' does not exist.");
		}
	}
}