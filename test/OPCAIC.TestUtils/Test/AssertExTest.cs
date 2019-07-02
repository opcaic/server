using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace OPCAIC.TestUtils.Test
{
	public class AssertExTest
	{
		[Fact]
		public void WaitsCorrectly()
		{
			ManualResetEventSlim handle = new ManualResetEventSlim(false);
			Task.Run(() => handle.Set());
			AssertEx.WaitForEvent(handle, 10);
		}

		[Fact]
		public void ThrowsWhenTimeElapsed()
		{
			ManualResetEventSlim handle = new ManualResetEventSlim(false);
			Assert.Throws<XunitException>(() => AssertEx.WaitForEvent(handle, 10));
		}
	}
}
