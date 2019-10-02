using Microsoft.Extensions.Logging;
using OPCAIC.Common;

namespace OPCAIC.Application.Logging
{
	public static class UserLoggingExtensions
	{
		public static void UserUpdated<TDto>(this ILogger logger, long id, TDto dto)
		{
			logger.LogInformation(LoggingEvents.UserUpdated, $"User {{{LoggingTags.UserId}}} updated: {{@{LoggingTags.UpdateData}}}", id, dto);
		}
	}
}