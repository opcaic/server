using System;
using System.Diagnostics;
using System.Threading;
using Xunit.Sdk;

namespace OPCAIC.Messaging.Test
{
	public static class AssertEx
	{
		public static void WaitForEvent(ManualResetEventSlim handle, double milliseconds)
		{
			if (Debugger.IsAttached)
			{
				milliseconds = 1000 * 60 * 60;
			}

			if (!handle.Wait(TimeSpan.FromMilliseconds(milliseconds)))
			{
				throw new XunitException("The event was not signaled in given time");
			}
		}
	}
}
