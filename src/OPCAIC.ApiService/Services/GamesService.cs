using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Schema;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.Application;
using OPCAIC.Application.Dtos.Games;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Domain.Entities;
using ValidationError = OPCAIC.Application.Infrastructure.Validation.ValidationError;

namespace OPCAIC.ApiService.Services
{
	public class GamesService : IGamesService
	{
		private readonly IGameRepository gameRepository;
		private readonly ILogger<GamesService> logger;
		private readonly IMapper mapper;

		public GamesService(IGameRepository gameRepository, IMapper mapper,
			ILogger<GamesService> logger)
		{
			this.gameRepository = gameRepository;
			this.mapper = mapper;
			this.logger = logger;
		}

		/// <inheritdoc />
		public async Task<long> CreateAsync(NewGameModel game, CancellationToken cancellationToken)
		{
			if (await gameRepository.ExistsOtherByNameAsync(game.Name, null, cancellationToken))
			{
				throw new ConflictException(
					new ValidationError(ValidationErrorCodes.GameNameConflict, null,
						nameof(game.Name)));
			}

			var dto = mapper.Map<NewGameDto>(game);

			var id = await gameRepository.CreateAsync(dto, cancellationToken);
			logger.GameCreated(id, dto);
			return id;
		}

		/// <inheritdoc />
		public async Task<GameDetailModel> GetByIdAsync(long id,
			CancellationToken cancellationToken)
		{
			var dto = await gameRepository.FindByIdAsync(id, cancellationToken);

			if (dto == null)
			{
				throw new NotFoundException(nameof(Game), id);
			}

			return mapper.Map<GameDetailModel>(dto);
		}

		/// <inheritdoc />
		public async Task UpdateAsync(long id, UpdateGameModel model,
			CancellationToken cancellationToken)
		{
			if (model.ConfigurationSchema != null &&
				!model.ConfigurationSchema.IsValid(JsonSchemaDefinition.Version7,
					out IList<string> messages))
			{
				throw new BadRequestException(ValidationErrorCodes.InvalidSchema,
					string.Join("\n", messages), nameof(UpdateGameModel.ConfigurationSchema));
			}

			if (await gameRepository.ExistsOtherByNameAsync(model.Name, id, cancellationToken))
			{
				throw new ConflictException(
					new ValidationError(ValidationErrorCodes.GameNameConflict, null,
						nameof(model.Name)));
			}

			var dto = mapper.Map<UpdateGameDto>(model);

			if (!await gameRepository.UpdateAsync(id, dto, cancellationToken))
			{
				throw new NotFoundException(nameof(Game), id);
			}

			logger.GameUpdated(id, dto);
		}
	}
}