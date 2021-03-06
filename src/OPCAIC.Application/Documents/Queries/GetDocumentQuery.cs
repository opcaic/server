﻿using AutoMapper;
using MediatR;
using OPCAIC.Application.Documents.Models;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Documents.Queries
{
	public class GetDocumentQuery : EntityRequestQuery<Document>, IRequest<DocumentDto>
	{
		public GetDocumentQuery(long documentId)
			:base(documentId)
		{
		}

		public class Handler : EntityRequestHandler<GetDocumentQuery, DocumentDto>
		{
			public Handler(IMapper mapper, IRepository<Document> repository)
				: base(mapper, repository)
			{
			}
		}

	}
}