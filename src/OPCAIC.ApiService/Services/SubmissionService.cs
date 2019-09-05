using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Submissions;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Services;

namespace OPCAIC.ApiService.Services
{
	public class SubmissionService : ISubmissionService
	{
		private readonly IMapper mapper;
		private readonly ISubmissionRepository repository;
		private readonly ITournamentRepository tournamentRepository;
		private readonly IStorageService storage;

		public SubmissionService(IMapper mapper, ISubmissionRepository repository, IStorageService storage, ITournamentRepository tournamentRepository)
		{
			this.repository = repository;
			this.storage = storage;
			this.tournamentRepository = tournamentRepository;
			this.mapper = mapper;
		}

		public async Task<long> CreateAsync(NewSubmissionModel model, long userId,
			CancellationToken cancellationToken)
		{
			// check whether the tournament can still accept submissions
			var tournament =
				await tournamentRepository.FindByIdAsync(model.TournamentId, cancellationToken);

			if (tournament.Deadline.HasValue &&
				tournament.Deadline.Value < DateTime.Now ||
				tournament.State != TournamentState.Published)
			{
				throw new BadRequestException(ValidationErrorCodes.TournamentDeadlinePassed, "Deadline for tournament registration has passed", null);
			}

			// save db entity
			var dto = mapper.Map<NewSubmissionDto>(model);
			dto.AuthorId = userId;
			var id = await repository.CreateAsync(dto, cancellationToken);

			var storeDto = await repository.FindSubmissionForStorageAsync(id, cancellationToken);

			// save archive
			using (var stream = storage.WriteSubmissionArchive(storeDto))
			{
				// TODO: do we really want to permit cancellation here?
				// TODO: connect db transactions and filesystem transactions storage
				await model.Archive.CopyToAsync(stream, cancellationToken);
			}

			return id;
		}

		public async Task<ListModel<SubmissionPreviewModel>> GetByFilterAsync(
			SubmissionFilterModel filter, CancellationToken cancellationToken)
		{
			var filterDto = mapper.Map<SubmissionFilterDto>(filter);

			var list = await repository.GetByFilterAsync(filterDto, cancellationToken);

			return mapper.Map<ListModel<SubmissionPreviewModel>>(list);
		}

		public async Task<SubmissionDetailModel> GetByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			var dto = await repository.FindByIdAsync(id, cancellationToken);

			if (dto == null)
			{
				throw new NotFoundException(nameof(Submission), id);
			}

			return mapper.Map<SubmissionDetailModel>(dto);
		}

		public async Task<Stream> GetSubmissionArchiveAsync(long id, CancellationToken cancellationToken)
		{
			var dto = await repository.FindSubmissionForStorageAsync(id, cancellationToken);

			if (dto == null)
			{
				throw new NotFoundException(nameof(Submission), id);
			}

			return storage.ReadSubmissionArchive(dto);
		}

		public async Task UpdateAsync(long id, UpdateSubmissionModel model,
			CancellationToken cancellationToken)
		{
			var dto = mapper.Map<UpdateSubmissionDto>(model);

			if (!await repository.UpdateAsync(id, dto, cancellationToken))
			{
				throw new NotFoundException(nameof(Submission), id);
			}
		}

		/// <inheritdoc />
		public Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken)
		{
			return repository.ExistsByIdAsync(id, cancellationToken);
		}
	}
}