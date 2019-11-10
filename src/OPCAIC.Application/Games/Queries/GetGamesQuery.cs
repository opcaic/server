using System;
using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Games.Models;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Games.Queries
{
	public class GetGamesQuery : FilterDtoBase, IRequest<PagedResult<GamePreviewDto>>
	{
		public const string SortByName = "name";
		public const string SortByCreated = "created";
		public const string SortByTournaments = "activeTournamentsCount";

		public string Name { get; set; }

		public class Validator : FilterValidator<GetGamesQuery>
		{
			public Validator()
			{
				RuleFor(m => m.Name).MinLength(1);
			}
		}

		public class Handler : FilterQueryHandler<GetGamesQuery, Game, GamePreviewDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<Game> repository) : base(mapper, repository)
			{
			}

			/// <inheritdoc />
			protected override void ApplyUserFilter(
				ProjectingSpecification<Game, GamePreviewDto> spec, GetGamesQuery request)
			{
				// all games are considered public by default
			}

			/// <inheritdoc />
			protected override void SetupSpecification(GetGamesQuery request,
				ProjectingSpecification<Game, GamePreviewDto> spec)
			{
				if (request.Name != null)
				{
					spec.AddCriteria(row => row.Name.ToUpper().StartsWith(request.Name.ToUpper()));
				}

				ApplyOrdering(spec, request);
			}

			private void ApplyOrdering(ProjectingSpecification<Game, GamePreviewDto> spec, GetGamesQuery request)
			{
				switch (request.SortBy)
				{
					case SortByCreated:
						spec.Ordered(row => row.Created, request.Asc);
						break;
					case SortByName:
						spec.Ordered(row => row.Name, request.Asc);
						break;
					case SortByTournaments:
						spec.OrderedProjection(row => row.ActiveTournamentsCount, request.Asc);
						break;
					default:
						spec.Ordered(row => row.Id, request.Asc);
						break;
				}
			}
		}
	}
}