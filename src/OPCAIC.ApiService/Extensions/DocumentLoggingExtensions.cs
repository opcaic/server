using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OPCAIC.Infrastructure.Dtos.Documents;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Extensions
{
	internal static class DocumentLoggingExtensions
	{
		public static void DocumentCreated(this ILogger logger, long id, NewDocumentDto dto)
		{
			logger.LogInformation(LoggingEvents.DocumentCreated,
				$"New Document '{dto.Name}' for tournament {{{LoggingTags.TournamentId}}} was created with id {{{LoggingTags.DocumentId}}}", dto.TournamentId, id);
		}

		public static void DocumentUpdated(this ILogger logger, long id, UpdateDocumentDto dto)
		{
			logger.LogInformation(LoggingEvents.DocumentUpdated,
				$"Document {{{LoggingTags.DocumentId}}} was updated: {{{LoggingTags.UpdateData}}}",
				id, JsonConvert.SerializeObject(dto));
		}
	}
}