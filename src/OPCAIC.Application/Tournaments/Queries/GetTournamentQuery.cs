using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Utils;

namespace OPCAIC.Application.Tournaments.Queries
{
	public class GetTournamentQuery : EntityRequestQuery<Tournament>, IRequest<TournamentDetailDto>
	{
		/// <inheritdoc />
		public GetTournamentQuery(long id) : base(id)
		{
		}

		public class Handler : IRequestHandler<GetTournamentQuery, TournamentDetailDto>
		{
			private readonly IMapper mapper;

			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<Tournament> repository)
			{
				this.mapper = mapper;
				this.repository = repository;
			}

			private readonly IRepository<Tournament> repository;

			/// <inheritdoc />
			public async Task<TournamentDetailDto> Handle(GetTournamentQuery request, CancellationToken cancellationToken)
			{
				// perform the menu mapping in-memory in order to successfully map inheritance
				var tournamentMap = mapper.GetMapExpression<Tournament, TournamentDetailDto>();
				var data = await repository.GetAsync(request.Id, Rebind.Map((Tournament t) => new
				{
					dto = Rebind.Invoke(t, tournamentMap),
					menu = t.MenuItems
				}), cancellationToken);

				data.dto.MenuItems = mapper.Map<List<MenuItemDto>>(data.menu);
				return data.dto;
			}
		}
	}
}