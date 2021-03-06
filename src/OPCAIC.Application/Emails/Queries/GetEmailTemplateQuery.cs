﻿using AutoMapper;
using MediatR;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enumerations;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using OPCAIC.Application.Infrastructure.Validation;

namespace OPCAIC.Application.Emails.Queries
{
	public class GetEmailTemplateQuery : IRequest<EmailTemplateDto>
	{
		public EmailType Name { get; set; }

		public LocalizationLanguage LanguageCode { get; set; }

		public class Validator : AbstractValidator<GetEmailTemplatesQuery>
		{
			public Validator()
			{
				RuleFor(m => m.Name).Required();
				RuleFor(m => m.LanguageCode).Required();
			}
		}

		public class Handler : IRequestHandler<GetEmailTemplateQuery, EmailTemplateDto>
		{
			private readonly IMapper mapper;
			private readonly IRepository<EmailTemplate> repository;

			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<EmailTemplate> repository)
			{
				this.mapper = mapper;
				this.repository = repository;
			}

			/// <inheritdoc />
			public async Task<EmailTemplateDto> Handle(GetEmailTemplateQuery request,
				CancellationToken cancellationToken)
			{
				var template = await repository.GetAsync(
					et => (et.LanguageCode == request.LanguageCode && et.Name == request.Name),
					cancellationToken);

				var dto = mapper.Map<EmailTemplateDto>(template);
				dto.TemplateVariables = request.Name.TemplateVariables;

				return dto;
			}
		}
	}
}