using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace OPCAIC.TestUtils
{
	public static class LoggerAssertions
	{
		public static void VerifyLogException<TException>(this Mock<ILogger> logger, LogLevel level)
			where TException : Exception
			=> logger.Verify(l => l.Log(
				level,
				It.IsAny<EventId>(),
				It.IsAny<object>(),
				It.IsAny<TException>(),
				It.IsAny<Func<object, Exception, string>>()));
	}
}