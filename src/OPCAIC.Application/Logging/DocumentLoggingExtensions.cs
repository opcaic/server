using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OPCAIC.Application.Documents.Commands;
using OPCAIC.Common;

namespace OPCAIC.Application.Logging
{
	public static class DocumentLoggingExtensions
	{
		public static void DocumentCreated(this ILogger logger, long id, CreateDocumentCommand dto)
		{
			logger.LogInformation(LoggingEvents.DocumentCreated,
				$"New Document '{dto.Name}' for tournament {{{LoggingTags.TournamentId}}} was created with id {{{LoggingTags.DocumentId}}}", dto.TournamentId, id);
		}

		public static void DocumentUpdated(this ILogger logger, UpdateDocumentCommand dto)
		{
			logger.LogInformation(LoggingEvents.DocumentUpdated,
				$"Document {{{LoggingTags.DocumentId}}} was updated: {{{LoggingTags.UpdateData}}}",
				dto.Id, JsonConvert.SerializeObject(dto));
		}
	}
}