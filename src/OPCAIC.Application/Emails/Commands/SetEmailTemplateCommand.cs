using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enumerations;

namespace OPCAIC.Application.Emails.Commands
{
	public class SetEmailTemplateCommand : IRequest, IMapTo<EmailTemplate>
	{
		public EmailType Name { get; set; }

		public LocalizationLanguage LanguageCode { get; set; }

		public string SubjectTemplate { get; set; }

		public string BodyTemplate { get; set; }

		public class Validator : AbstractValidator<SetEmailTemplateCommand>
		{
			public Validator()
			{
				RuleFor(m => m.LanguageCode).Required();
				RuleFor(m => m.Name).Required();
			}
		}

		public class Handler : IRequestHandler<SetEmailTemplateCommand>
		{
			private readonly ILogger<SetEmailTemplateCommand> logger;
			private readonly IMapper mapper;
			private readonly IRepository<EmailTemplate> repository;

			public Handler(ILogger<SetEmailTemplateCommand> logger, IMapper mapper, IRepository<EmailTemplate> repository)
			{
				this.logger = logger;
				this.mapper = mapper;
				this.repository = repository;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(SetEmailTemplateCommand request, CancellationToken cancellationToken)
			{
				var template = await repository.FindAsync(t
							=> t.Name == request.Name && t.LanguageCode == request.LanguageCode,
						cancellationToken);

				bool created = template == null;
				if (template == null)
				{
					template = new EmailTemplate();
					repository.Add(template);
				}

				mapper.Map(request, template);
				await repository.SaveChangesAsync(cancellationToken);

				if (created)
				{
					logger.EmailTemplateCreated(request.Name, request.LanguageCode);
				}
				else
				{
					logger.EmailTemplateUpdated(request.Name, request.LanguageCode);
				}

				return Unit.Value;
			}
		}
	}
}