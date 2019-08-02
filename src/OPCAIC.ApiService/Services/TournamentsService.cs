using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Services
{
	public class TournamentsService : ITournamentsService
	{
		private readonly IMapper mapper;
		private readonly ITournamentRepository tournamentRepository;

		public TournamentsService(ITournamentRepository tournamentRepository, IMapper mapper)
		{
			this.tournamentRepository = tournamentRepository;
			this.mapper = mapper;
		}

		/// <inheritdoc />
		public async Task<long> CreateAsync(NewTournamentModel tournament, CancellationToken cancellationToken)
		{
			// TODO: check if game with a given id exists, also maybe check enums

			var dto = mapper.Map<NewTournamentDto>(tournament);

			return await tournamentRepository.CreateAsync(dto, cancellationToken);
		}

		/// <inheritdoc />
		public async Task<ListModel<TournamentPreviewModel>> GetByFilterAsync(TournamentFilterModel filter,
			CancellationToken cancellationToken)
		{
			var filterDto = mapper.Map<TournamentFilterDto>(filter);

			var dto = await tournamentRepository.GetByFilterAsync(filterDto, cancellationToken);

			return new ListModel<TournamentPreviewModel>
			{
				Total = dto.Total,
				List = dto.List.Select(tournament => mapper.Map<TournamentPreviewModel>(tournament))
			};
		}

		/// <inheritdoc />
		public async Task<TournamentDetailModel> GetByIdAsync(long id, CancellationToken cancellationToken)
		{
			var dto = await tournamentRepository.FindByIdAsync(id, cancellationToken);

			if (dto == null)
			{
				throw new NotFoundException(nameof(Tournament), id);
			}

			return mapper.Map<TournamentDetailModel>(dto);
		}

		/// <inheritdoc />
		public async Task UpdateAsync(long id, UpdateTournamentModel model, CancellationToken cancellationToken)
		{
			// TODO: check if game with a given id exists, also maybe check enums

			var dto = mapper.Map<UpdateTournamentDto>(model);

			if (!await tournamentRepository.UpdateAsync(id, dto, cancellationToken))
			{
				throw new NotFoundException(nameof(Tournament), id);
			}
		}
	}
}