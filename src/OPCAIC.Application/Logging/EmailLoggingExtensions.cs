using Microsoft.Extensions.Logging;
using OPCAIC.Application.Emails.Commands;
using OPCAIC.Common;
using OPCAIC.Domain.Enumerations;

namespace OPCAIC.Application.Logging
{
	public static class EmailLoggingExtensions
	{
		public static void EmailTemplateDeleted(this ILogger logger, string type, string languageCode)
		{
			logger.LogInformation(LoggingEvents.EmailTemplateDeleted, $"Email template {{{LoggingTags.EmailTemplateType}}}/{{{LoggingTags.LanguageCode}}} deleted.", type, languageCode);
		}

		public static void EmailTemplateCreated(this ILogger logger, string type, string languageCode)
		{
			logger.LogInformation(LoggingEvents.EmailTemplateCreated, $"Email template {{{LoggingTags.EmailTemplateType}}}/{{{LoggingTags.LanguageCode}}} created.", type, languageCode);
		}

		public static void EmailTemplateUpdated(this ILogger logger, string type, string languageCode)
		{
			logger.LogInformation(LoggingEvents.EmailTemplateUpdated, $"Email template {{{LoggingTags.EmailTemplateType}}}/{{{LoggingTags.LanguageCode}}} updated.", type, languageCode);
		}

		public static void EmailTemplateMissing(this ILogger logger, string type, string languageCode)
		{
			logger.LogWarning(LoggingEvents.EmailTemplateMissing, $"Cannot find template for {{{LoggingTags.EmailTemplateType}}} for language {{{LoggingTags.LanguageCode}}}, fallback to {LocalizationLanguage.EN}",
				type, languageCode);
		}
	}
}