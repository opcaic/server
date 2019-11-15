using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Utils;

namespace OPCAIC.Application.Tournaments.Queries
{
	public class GetTournamentAdminQuery : GetTournamentQueryBase<TournamentAdminDto>
	{
		/// <inheritdoc />
		public GetTournamentAdminQuery(long id) : base(id)
		{
		}

		public class Handler : HandlerBase<GetTournamentAdminQuery>
		{
			private readonly IStorageService storage;

			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<Tournament> repository, IStorageService storage) : base(mapper, repository)
			{
				this.storage = storage;
			}

			/// <inheritdoc />
			protected override void PostProcess(TournamentAdminDto dto)
			{
				using var archive = storage.ReadTournamentAdditionalFiles(dto.Id);
				dto.AdditionalFilesLength = archive?.Length;
				base.PostProcess(dto);
			}
		}
	}
	public class GetTournamentQuery : GetTournamentQueryBase<TournamentDetailDto>
	{
		/// <inheritdoc />
		public GetTournamentQuery(long id) : base(id)
		{
		}

		public class Handler : HandlerBase<GetTournamentQuery>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<Tournament> repository) : base(mapper, repository)
			{
			}
		}
	}

	public abstract class GetTournamentQueryBase<TDto> : EntityRequestQuery<Tournament>, IRequest<TDto>
		where TDto : TournamentDetailDto
	{
		public class HandlerBase<TRequest> : IRequestHandler<TRequest, TDto>
			where TRequest : GetTournamentQueryBase<TDto>
		{
			private readonly IMapper mapper;
			private readonly IRepository<Tournament> repository;

			/// <inheritdoc />
			public HandlerBase(IMapper mapper, IRepository<Tournament> repository)
			{
				this.mapper = mapper;
				this.repository = repository;
			}

			/// <inheritdoc />
			public async Task<TDto> Handle(TRequest request, CancellationToken cancellationToken)
			{
				// perform the menu mapping in-memory in order to successfully map inheritance
				var tournamentMap = mapper.GetMapExpression<Tournament, TDto>();
				var data = await repository.GetAsync(request.Id, Rebind.Map((Tournament t) => new
				{
					dto = Rebind.Invoke(t, tournamentMap),
					menu = t.MenuItems
				}), cancellationToken);

				data.dto.MenuItems = mapper.Map<List<MenuItemDto>>(data.menu);
				PostProcess(data.dto);
				return data.dto;
			}

			protected virtual void PostProcess(TDto dto)
			{
			}
		}

		/// <inheritdoc />
		protected GetTournamentQueryBase(long id) : base(id)
		{
		}
	}
}