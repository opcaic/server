using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Logging;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enumerations;

namespace OPCAIC.Application.Emails.Commands
{
	public class DeleteEmailTemplateCommand : IRequest
	{
		public EmailType Name { get; set; }

		public LocalizationLanguage LanguageCode { get; set; }

		public class Validator : AbstractValidator<DeleteEmailTemplateCommand>
		{
			public Validator()
			{
				RuleFor(m => m.LanguageCode) .Required();
				RuleFor(m => m.Name) .Required();
			}
		}

		public class Handler : IRequestHandler<DeleteEmailTemplateCommand>
		{
			private readonly ILogger<DeleteEmailTemplateCommand> logger;
			private readonly IRepository<EmailTemplate> repository;

			public Handler(ILogger<DeleteEmailTemplateCommand> logger, IRepository<EmailTemplate> repository)
			{
				this.logger = logger;
				this.repository = repository;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(DeleteEmailTemplateCommand request, CancellationToken cancellationToken)
			{
				if (!await repository.DeleteAsync(t
						=> t.Name == request.Name && t.LanguageCode == request.LanguageCode,
					cancellationToken))
				{
					throw new NotFoundException(nameof(EmailTemplate), $"No template '{request.Name}' for language '{request.LanguageCode}' exists.");
				}

				logger.EmailTemplateDeleted(request.Name, request.LanguageCode);
				return Unit.Value;
			}
		}
	}
}