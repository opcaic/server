using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Application.Documents.Models;
using OPCAIC.Application.Dtos.Documents;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Repositories
{
	public class DocumentRepository
		: GenericRepository<Document, DocumentDto, NewDocumentDto, UpdateDocumentDto>,
			IDocumentRepository
	{
		/// <inheritdoc />
		public DocumentRepository(DataContext context, IMapper mapper)
			: base(context, mapper)
		{
		}

		/// <inheritdoc />
		public Task<DocumentAuthDto> GetAuthorizationData(long id,
			CancellationToken cancellationToken = default)
		{
			return DbSet.Where(d => d.Id == id)
				.Select(d => new DocumentAuthDto
				{
					TournamentOwnerId = d.Tournament.OwnerId,
					TournamentManagersIds =
						d.Tournament.Managers.Select(m => m.UserId).ToArray()
				})
				.SingleOrDefaultAsync(cancellationToken);
		}
	}
}