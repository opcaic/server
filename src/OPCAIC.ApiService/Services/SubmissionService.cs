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
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;
using OPCAIC.Infrastructure.Dtos.Tournaments;
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

			if (!CanTournamentAcceptSubmissions(tournament))
			{
				if (tournament.Deadline.HasValue && tournament.Deadline < DateTime.Now)
				{
					throw new BadRequestException(ValidationErrorCodes.TournamentDeadlinePassed, "Deadline for tournament registration has passed.", null);
				}

				throw new BadRequestException(ValidationErrorCodes.TournamentDoesNotAcceptSubmission, "This tournament does not accept submissions anymore.", null);
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

			var mapped = mapper.Map<ListModel<SubmissionPreviewModel>>(list);

			for (var i = 0; i < list.List.Count; i++)
			{
				mapped.List[i].ValidationState = GetValidationState(list.List[i].LastValidation);
			}

			return mapped;
		}

		private static SubmissionValidationState GetValidationState(SubmissionValidationDto validation)
		{
			SubmissionValidationState target;
			if (!validation.Executed.HasValue)
			{
				target = SubmissionValidationState.Queued;
			}

			else
			{
				switch (validation.ValidatorResult)
				{
					case EntryPointResult.Success:
						target = SubmissionValidationState.Valid;
						break;

					case EntryPointResult.UserError:
						target = SubmissionValidationState.Invalid;
						break;

					case EntryPointResult.NotExecuted: // validation ended in earlier stage
					case EntryPointResult.Cancelled:
					case EntryPointResult.ModuleError:
					case EntryPointResult.PlatformError:
						target = SubmissionValidationState.Error;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			return target;
		}

		public async Task<SubmissionDetailModel> GetByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			var dto = await repository.FindByIdAsync(id, cancellationToken);

			if (dto == null)
			{
				throw new NotFoundException(nameof(Submission), id);
			}

			var mapped = mapper.Map<SubmissionDetailModel>(dto);
			mapped.ValidationState = GetValidationState(dto.LastValidation);
			return mapped;
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

		public static bool CanTournamentAcceptSubmissions(TournamentDetailDto tournament)
		{
			return tournament.State == TournamentState.Published &&
				(tournament.Deadline == null || tournament.Deadline > DateTime.Now) ||
				tournament.State == TournamentState.Running &&
				tournament.Scope == TournamentScope.Ongoing;
		}

		/// <inheritdoc />
		public Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken)
		{
			return repository.ExistsByIdAsync(id, cancellationToken);
		}
	}
}