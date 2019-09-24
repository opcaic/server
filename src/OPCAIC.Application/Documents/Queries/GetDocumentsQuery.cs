using System;
using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Dtos.Documents;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

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
			public Handler(IMapper mapper, IDocumentRepository repository) : base(mapper,
				repository)
			{
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