using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Matches;
using OPCAIC.Domain.Entities;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Services
{
	public class MatchService : IMatchService
	{
		private readonly IMatchRepository matchRepository;
		private readonly IMapper mapper;

		public MatchService(IMatchRepository matchRepository, IMapper mapper)
		{
			this.matchRepository = matchRepository;
			this.mapper = mapper;
		}

		/// <inheritdoc />
		public async Task<ListModel<MatchDetailModel>> GetByFilterAsync(MatchFilterModel filter,
			CancellationToken cancellationToken)
		{
			var filterDto = mapper.Map<MatchFilterDto>(filter);

			var dto = await matchRepository.GetByFilterAsync(filterDto, cancellationToken);

			return mapper.Map<ListModel<MatchDetailModel>>(dto);
		}

		/// <inheritdoc />
		public async Task<MatchDetailModel> GetByIdAsync(long id, CancellationToken cancellationToken)
		{
			var dto = await matchRepository.FindByIdAsync(id, cancellationToken);

			if (dto == null)
			{
                throw new NotFoundException(nameof(Match), id);
			}

			return mapper.Map<MatchDetailModel>(dto);
		}
	}
}
