using System;
using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Documents.Models;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Queries;
using OPCAIC.Domain.Entities;
using OPCAIC.Utils;

namespace OPCAIC.Application.Documents.Queries
{
	public class GetDocumentsQuery : FilterDtoBase, IRequest<PagedResult<DocumentDto>>
	{
		public const string SortByName = "name";
		public const string SortByCreated = "created";

		public string Name { get; set; }

		public long? TournamentId { get; set; }

		public class Validator : FilterValidator<GetDocumentsQuery>
		{
			public Validator()
			{
				RuleFor(m => m.Name).MinLength(1);
			}
		}

		public class Handler : FilterQueryHandler<GetDocumentsQuery, Document, DocumentDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<Document> repository) : base(mapper,
				repository)
			{
			}

			/// <inheritdoc />
			protected override void ApplyUserFilter(ProjectingSpecification<Document, DocumentDto> spec, long? userId)
			{
				// if user can see the tournament, then he should be able to see the documents
				var tournamentCriteria = GetTournamentsQuery.Handler.GetUserFilter(userId);
				spec.AddCriteria(Rebind.Map((Document d)
					=> Rebind.Invoke(d.Tournament, tournamentCriteria)));
			}

			/// <inheritdoc />
			protected override void SetupSpecification(GetDocumentsQuery request,
				ProjectingSpecification<Document, DocumentDto> spec)
			{
				if (request.Name != null)
				{
					spec.AddCriteria(row => row.Name.ToUpper().StartsWith(request.Name.ToUpper()));
				}

				if (request.TournamentId != null)
				{
					spec.AddCriteria(row => row.TournamentId == request.TournamentId);
				}

				spec.Ordered(GetSortingKey(request.SortBy), request.Asc);
			}

			private Expression<Func<Document, object>> GetSortingKey(string key)
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