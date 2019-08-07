using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Documents;
using OPCAIC.Infrastructure.Dtos.Documents;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Services
{
	public class DocumentService : IDocumentService
	{
		private readonly IDocumentRepository documentRepository;
		private readonly IMapper mapper;

		public DocumentService(IDocumentRepository documentRepository, IMapper mapper)
		{
			this.documentRepository = documentRepository;
			this.mapper = mapper;
		}

		/// <inheritdoc />
		public async Task<long> CreateAsync(NewDocumentModel document,
			CancellationToken cancellationToken)
		{
			var dto = mapper.Map<NewDocumentDto>(document);

			return await documentRepository.CreateAsync(dto, cancellationToken);
		}

		/// <inheritdoc />
		public async Task<ListModel<DocumentDetailModel>> GetByFilterAsync(
			DocumentFilterModel filter,
			CancellationToken cancellationToken)
		{
			var filterDto = mapper.Map<DocumentFilterDto>(filter);

			var dto = await documentRepository.GetByFilterAsync(filterDto, cancellationToken);

			return new ListModel<DocumentDetailModel>
			{
				Total = dto.Total,
				List = dto.List.Select(tournament
					=> mapper.Map<DocumentDetailModel>(tournament))
			};
		}

		/// <inheritdoc />
		public async Task<DocumentDetailModel> GetByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			var dto = await documentRepository.FindByIdAsync(id, cancellationToken);

			if (dto == null)
			{
				throw new NotFoundException(nameof(Document), id);
			}

			return mapper.Map<DocumentDetailModel>(dto);
		}

		/// <inheritdoc />
		public async Task UpdateAsync(long id, UpdateDocumentModel model,
			CancellationToken cancellationToken)
		{
			// TODO: check if there is a document with that id

			var dto = mapper.Map<UpdateDocumentDto>(model);

			if (!await documentRepository.UpdateAsync(id, dto, cancellationToken))
			{
				throw new NotFoundException(nameof(Tournament), id);
			}
		}
	}
}