using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Specifications;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Documents.Commands
{
	public class UpdateDocumentCommand : IIdentifiedRequest, IRequest, IMapTo<Document>
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public string Content { get; set; }

		public class Validator : AbstractValidator<UpdateDocumentCommand>
		{
			public Validator()
			{
				RuleFor(m => m.Name).MaxLength(StringLengths.DocumentName).Required();
			}
		}

		public class Handler : IRequestHandler<UpdateDocumentCommand>
		{
			private readonly ILogger<UpdateDocumentCommand> logger;
			private readonly IRepository<Document> repository;

			/// <inheritdoc />
			public Handler(IRepository<Document> repository, ILogger<UpdateDocumentCommand> logger)
			{
				this.repository = repository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(UpdateDocumentCommand request, CancellationToken cancellationToken)
			{
				await repository.UpdateAsync(request.Id, request, cancellationToken);
				logger.DocumentUpdated(request);
				return Unit.Value;
			}
		}
	}
}