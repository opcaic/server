using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Documents;
using OPCAIC.Application.Dtos.Documents;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;

namespace OPCAIC.ApiService.Services
{
	public class DocumentService : IDocumentService
	{
		private readonly IDocumentRepository documentRepository;
		private readonly IMapper mapper;
		private readonly ITournamentRepository tournamentRepository;
		private readonly ILogger<DocumentService> logger;

		public DocumentService(IDocumentRepository documentRepository,
			ITournamentRepository tournamentRepository, IMapper mapper, ILogger<DocumentService> logger)
		{
			this.documentRepository = documentRepository;
			this.tournamentRepository = tournamentRepository;
			this.mapper = mapper;
			this.logger = logger;
		}

		/// <inheritdoc />
		public async Task<long> CreateAsync(NewDocumentModel document,
			CancellationToken cancellationToken)
		{
			if (!await tournamentRepository.ExistsByIdAsync(document.TournamentId,
				cancellationToken))
			{
				throw new NotFoundException(nameof(Tournament), document.TournamentId);
			}

			var dto = mapper.Map<NewDocumentDto>(document);

			var id = await documentRepository.CreateAsync(dto, cancellationToken);
			logger.DocumentCreated(id, dto);
			return id;
		}

		/// <inheritdoc />
		public async Task<ListModel<DocumentDetailModel>> GetByFilterAsync(
			DocumentFilterModel filter,
			CancellationToken cancellationToken)
		{
			var filterDto = mapper.Map<DocumentFilterDto>(filter);

			var dto = await documentRepository.GetByFilterAsync(filterDto, cancellationToken);

			return mapper.Map<ListModel<DocumentDetailModel>>(dto);
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
			var dto = mapper.Map<UpdateDocumentDto>(model);

			if (!await documentRepository.UpdateAsync(id, dto, cancellationToken))
			{
				throw new NotFoundException(nameof(Document), id);
			}

			logger.DocumentUpdated(id, dto);
		}

		/// <inheritdoc />
		public async Task DeleteAsync(long id, CancellationToken cancellationToken)
		{
			if (!await documentRepository.DeleteAsync(id, cancellationToken))
			{
				throw new NotFoundException(nameof(Document), id);
			}
		}
	}
}