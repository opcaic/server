using System;
using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Games.Models;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Queries;
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

		public class Handler : FilterQueryHandler<GetGamesQuery, Game, GamePreviewModel>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IGameRepository repository) : base(mapper, repository)
			{
			}

			/// <inheritdoc />
			protected override void ApplyUserFilter(ProjectingSpecification<Game, GamePreviewModel> spec, long? userId)
			{
				// all games are considered public by default
			}

			/// <inheritdoc />
			protected override void SetupSpecification(GetGamesQuery request,
				ProjectingSpecification<Game, GamePreviewModel> spec)
			{
				if (request.Name != null)
				{
					spec.AddCriteria(row => row.Name.ToUpper().StartsWith(request.Name.ToUpper()));
				}

				spec.Ordered(GetSortingKey(request.SortBy), request.Asc);
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