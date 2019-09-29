using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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
	}
}