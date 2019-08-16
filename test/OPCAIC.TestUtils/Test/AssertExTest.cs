using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace OPCAIC.TestUtils.Test
{
	public class AssertExTest
	{
		[Fact]
		public void ThrowsWhenTimeElapsed()
		{
			var handle = new ManualResetEventSlim(false);
			Assert.Throws<XunitException>(() => AssertEx.WaitForEvent(handle, 10));
		}

		[Fact]
		public void WaitsCorrectly()
		{
			var handle = new ManualResetEventSlim(false);
			Task.Run(() => handle.Set());
			AssertEx.WaitForEvent(handle, 10);
		}
	}
}