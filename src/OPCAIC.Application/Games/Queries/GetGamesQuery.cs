using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Games.Models;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Games.Queries
{
	public class GetGamesQuery : FilterDtoBase, IRequest<PagedResult<GamePreviewModel>>
	{
		public const string SortByName = "name";
		public const string SortByCreated = "created";
		public string Name { get; set; }

		public class Validator : FilterValidator<GetGamesQuery>
		{
			public Validator()
			{
				RuleFor(m => m.Name).MinLength(1);
			}
		}

		public class Handler : IRequestHandler<GetGamesQuery, PagedResult<GamePreviewModel>>
		{
			private readonly IGameRepository repository;
			private readonly IMapper mapper;

			public Handler(IGameRepository repository, IMapper mapper)
			{
				this.repository = repository;
				this.mapper = mapper;
			}

			/// <inheritdoc />
			public Task<PagedResult<GamePreviewModel>> Handle(GetGamesQuery request, CancellationToken cancellationToken)
			{
				var spec = ProjectingSpecification<Game>.Create<GamePreviewModel>(mapper);

				if (request.Name != null)
				{
					spec.AddCriteria(row => row.Name.ToUpper().StartsWith(request.Name.ToUpper()));
				}

				spec.Ordered(GetSortingKey(request.SortBy), request.Asc);

				return repository.ListPagedAsync(spec, cancellationToken);
			}

			private Expression<Func<Game, object>> GetSortingKey(string key)
			{
				switch (key)
				{
					case SortByCreated:
						return row => row.Created;
					case SortByName:
						return row => row.Name;
					default:
						return row => row.Id;
				}
			}
		}
	}
}