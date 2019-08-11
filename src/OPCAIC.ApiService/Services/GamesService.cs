using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.Infrastructure.Dtos.Games;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Services
{
	public class GamesService : IGamesService
	{
		private readonly IMapper mapper;
		private readonly IGameRepository gameRepository;

		public GamesService(IGameRepository gameRepository, IMapper mapper)
		{
			this.gameRepository = gameRepository;
			this.mapper = mapper;
		}

		/// <inheritdoc />
		public async Task<long> CreateAsync(NewGameModel game, CancellationToken cancellationToken)
		{
			if (await gameRepository.ExistsByName(game.Name, cancellationToken))
			{
				throw new ConflictException(new ValidationError(ValidationErrorCodes.GameNameConflict, null, nameof(game.Name)));
			}

			var dto = mapper.Map<NewGameDto>(game);

			return await gameRepository.CreateAsync(dto, cancellationToken);
		}

		/// <inheritdoc />
		public async Task<ListModel<GamePreviewModel>> GetByFilterAsync(GameFilterModel filter,
			CancellationToken cancellationToken)
		{
			var filterDto = mapper.Map<GameFilterDto>(filter);

			var dto = await gameRepository.GetByFilterAsync(filterDto, cancellationToken);

			return new ListModel<GamePreviewModel>
			{
				Total = dto.Total,
				List = dto.List.Select(game => mapper.Map<GamePreviewModel>(game))
			};
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
			if (await gameRepository.ExistsByName(model.Name, cancellationToken))
			{
				throw new ConflictException(new ValidationError(ValidationErrorCodes.GameNameConflict, null, nameof(model.Name)));
			}

			var dto = mapper.Map<UpdateGameDto>(model);

			if (!await gameRepository.UpdateAsync(id, dto, cancellationToken))
			{
				throw new NotFoundException(nameof(Game), id);
			}
		}
	}
}