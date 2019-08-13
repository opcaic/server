using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using Xunit.Abstractions;

namespace OPCAIC.TestUtils
{
	public class ThreadHelper
	{
		private readonly string description;
		private readonly ITestOutputHelper output;
		private Exception ex;
		private Action stopAction;
		private Thread thread;

		public ThreadHelper(ITestOutputHelper output, string description)
		{
			this.output = output;
			this.description = description;
		}

		public bool IsRunning => thread != null;

		public void Start(Action enter, Action stop)
		{
			stopAction = stop;

			thread = new Thread(() =>
			{
				try
				{
					enter();
				}
				catch (Exception e)
				{
					output.WriteLine($"Thread '{description}' exited with exception: \n{e}");
					ex = e;
				}
			});

			thread.Name = description;
			thread.Start();
		}

		public void Stop()
		{
			stopAction();
			if (!thread.Join(100))
			{
				output.WriteLine("Thread Aborted");
				thread.Abort();
			}

			if (ex != null)
			{
				// rethrow any exception with original stack trace
				ExceptionDispatchInfo.Throw(ex);
			}

			ex = null;
			thread = null;
			stopAction = null;
		}
	}
}