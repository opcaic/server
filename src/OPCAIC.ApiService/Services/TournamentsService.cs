using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json.Schema;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Services
{
	public class TournamentsService : ITournamentsService
	{
		private readonly IGameRepository gameRepository;
		private readonly IMapper mapper;
		private readonly ITournamentRepository tournamentRepository;

		public TournamentsService(ITournamentRepository tournamentRepository, IMapper mapper,
			IGameRepository gameRepository)
		{
			this.tournamentRepository = tournamentRepository;
			this.mapper = mapper;
			this.gameRepository = gameRepository;
		}

		/// <inheritdoc />
		public Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken)
		{
			return tournamentRepository.ExistsByIdAsync(id, cancellationToken);
		}

		public async Task StartTournamentEvaluation(long id, CancellationToken cancellationToken)
		{
			var detail = await tournamentRepository.FindByIdAsync(id, cancellationToken);
			if (detail.State != TournamentState.Published)
			{
				throw new BadRequestException(
					ValidationErrorCodes.TournamentEvaluationAlreadyStarted,
					"Only tournaments in Published state can be started", null);
			}

			await tournamentRepository.UpdateTournamentState(id,
				new TournamentStartedUpdateDto
				{
					State = TournamentState.Running, EvaluationStarted = DateTime.Now
				}, cancellationToken);
		}

		/// <inheritdoc />
		public async Task<long> CreateAsync(NewTournamentModel tournament,
			CancellationToken cancellationToken)
		{
			var dto = mapper.Map<NewTournamentDto>(tournament);

			var schema = JSchema.Parse(
				await gameRepository.GetConfigurationSchemaAsync(tournament.GameId,
					cancellationToken));

			if (!tournament.Configuration.IsValid(schema, out IList<string> messages))
			{
				throw new BadRequestException(ValidationErrorCodes.InvalidConfiguration,
					string.Join("\n", messages), nameof(tournament.Configuration));
			}

			return await tournamentRepository.CreateAsync(dto, cancellationToken);
		}

		/// <inheritdoc />
		public async Task<ListModel<TournamentPreviewModel>> GetByFilterAsync(
			TournamentFilterModel filter,
			CancellationToken cancellationToken)
		{
			var filterDto = mapper.Map<TournamentFilterDto>(filter);

			var dto = await tournamentRepository.GetByFilterAsync(filterDto, cancellationToken);

			return new ListModel<TournamentPreviewModel>
			{
				Total = dto.Total, List = dto.List.Select(mapper.Map<TournamentPreviewModel>)
			};
		}

		/// <inheritdoc />
		public async Task<TournamentDetailModel> GetByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			var dto = await tournamentRepository.FindByIdAsync(id, cancellationToken);

			if (dto == null)
			{
				throw new NotFoundException(nameof(Tournament), id);
			}

			return mapper.Map<TournamentDetailModel>(dto);
		}

		/// <inheritdoc />
		public async Task UpdateAsync(long id, UpdateTournamentModel model,
			CancellationToken cancellationToken)
		{
			var dto = mapper.Map<UpdateTournamentDto>(model);

			if (!await tournamentRepository.UpdateAsync(id, dto, cancellationToken))
			{
				throw new NotFoundException(nameof(Tournament), id);
			}
		}
	}
}