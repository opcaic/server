using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Specifications;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Documents.Commands
{
	public class CreateDocumentCommand : IRequest<long>, IMapTo<Document>
	{
		public string Name { get; set; }
		public string Content { get; set; }
		public long TournamentId { get; set; }

		public class Validator : AbstractValidator<CreateDocumentCommand>
		{
			public Validator()
			{
				RuleFor(m => m.Name).MaxLength(StringLengths.DocumentName).Required();
				RuleFor(m => m.TournamentId).EntityId(typeof(Tournament));
			}
		}

		public class Handler : IRequestHandler<CreateDocumentCommand, long>
		{
			private readonly ILogger<CreateDocumentCommand> logger;
			private readonly IMapper mapper;
			private readonly IRepository<Document> repository;

			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<Document> repository, ILogger<CreateDocumentCommand> logger)
			{
				this.mapper = mapper;
				this.repository = repository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<long> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
			{
				var document = mapper.Map<Document>(request);
				repository.Add(document);
				await repository.SaveChangesAsync(cancellationToken);
				logger.DocumentCreated(document.Id, request);

				return document.Id;
			}
		}
	}
}
