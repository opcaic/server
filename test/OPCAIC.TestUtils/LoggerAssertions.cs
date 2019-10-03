using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace OPCAIC.TestUtils
{
	public static class LoggerAssertions
	{
		public static void VerifyLogException<T,TException>(this Mock<ILogger<T>> logger, LogLevel level)
			where TException : Exception
		{
			logger.Verify(l => l.Log(
				level,
				It.IsAny<EventId>(),
				It.IsAny<object>(),
				It.IsAny<TException>(),
				It.IsAny<Func<object, Exception, string>>()));
		}
	}
}