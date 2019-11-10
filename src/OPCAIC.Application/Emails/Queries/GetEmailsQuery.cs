using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper;
using FluentValidation;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Emails.Models;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Emails.Queries
{
	public class GetEmailsQuery : FilterDtoBase, IRequest<PagedResult<EmailDto>>
	{
		public const string SortByRecipient = "recipientEmail";
		public const string SortByRemainingAttempts = "remainingAttempts";
		public const string SortByCreated = "created";

		public bool? Sent { get; set; }

		public string RecipientEmail { get; set; }

		public EmailType TemplateName { get; set; }

		public class Handler : FilterQueryHandler<GetEmailsQuery, Email, EmailDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<Email> repository) : base(mapper, repository)
			{
			}

			/// <inheritdoc />
			protected override void ApplyUserFilter(ProjectingSpecification<Email, EmailDto> spec,
				GetEmailsQuery request)
			{
				throw new BusinessException("Only admin may query emails");
			}

			/// <inheritdoc />
			protected override void SetupSpecification(GetEmailsQuery request, ProjectingSpecification<Email, EmailDto> spec)
			{
				if (request.Sent != null)
				{
					spec.AddCriteria(e => (e.SentAt != null) == request.Sent.Value);
				}

				if (request.RecipientEmail != null)
				{
					spec.AddCriteria(e => e.RecipientEmail == request.RecipientEmail);
				}

				if (request.TemplateName != null)
				{
					spec.AddCriteria(e => e.TemplateName == request.TemplateName);
				}

				spec.Ordered(GetSortingKey(request.SortBy), request.Asc);
			}

			protected Expression<Func<Email, object>> GetSortingKey(string key)
			{
				switch (key)
				{
					case SortByCreated:
						return e => e.Created;
					case SortByRecipient:
						return e => e.RecipientEmail;
					case SortByRemainingAttempts:
						return e => e.RemainingAttempts;
					default:
						return e => e.Id;
				}
			}
		}
	}
}