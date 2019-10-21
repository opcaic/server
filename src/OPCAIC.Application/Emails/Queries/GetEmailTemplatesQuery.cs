using AutoMapper;
using FluentValidation;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enumerations;

namespace OPCAIC.Application.Emails.Queries
{
	public class GetEmailTemplatesQuery : FilterDtoBase, IRequest<PagedResult<EmailTemplateDto>>
	{
		public LocalizationLanguage LanguageCode { get; set; }

		public EmailType Name { get; set; }

		public class Validator : FilterValidator<GetEmailTemplatesQuery>
		{
		}

		public class Handler : FilterQueryHandler<GetEmailTemplatesQuery, EmailTemplate, EmailTemplateDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<EmailTemplate> repository) : base(mapper, repository)
			{
			}

			/// <inheritdoc />
			protected override void ApplyUserFilter(ProjectingSpecification<EmailTemplate, EmailTemplateDto> spec, long? userId)
			{
				throw new BusinessException("Only admin may query email templates");
			}

			/// <inheritdoc />
			protected override void SetupSpecification(GetEmailTemplatesQuery request, ProjectingSpecification<EmailTemplate, EmailTemplateDto> spec)
			{
				if (request.LanguageCode != null)
				{
					spec.AddCriteria(t => t.LanguageCode == request.LanguageCode);
				}

				if (request.Name != null)
				{
					spec.AddCriteria(t => t.Name == request.Name);
				}
			}
		}
	}
}