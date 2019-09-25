using OPCAIC.Application.Documents.Models;
using OPCAIC.Application.Dtos.Documents;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IDocumentRepository
		: IGenericRepository<DocumentDto, NewDocumentDto, UpdateDocumentDto>,
			IDeleteRepository,
			IAuthDataRepository<DocumentAuthDto>,
			IRepository<Document>
	{
	}
}