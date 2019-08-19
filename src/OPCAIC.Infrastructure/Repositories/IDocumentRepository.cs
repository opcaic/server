﻿using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Documents;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IDocumentRepository
	{
		Task<long> CreateAsync(NewDocumentDto document, CancellationToken cancellationToken);

		Task<ListDto<DocumentDetailDto>> GetByFilterAsync(DocumentFilterDto filter,
			CancellationToken cancellationToken);

		Task<DocumentDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken);

		Task<bool> UpdateAsync(long id, UpdateDocumentDto document,
			CancellationToken cancellationToken);
		
		Task<bool> DeleteAsync(long id, CancellationToken cancellationToken);
	}
}