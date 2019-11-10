using System;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using FluentValidation;
using MediatR;
using OPCAIC.Application.Dtos;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Queries;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Users.Model;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Users.Queries
{
	public class GetUsersQuery : FilterDtoBase, IRequest<PagedResult<UserPreviewDto>>
	{
		public const string SortByUsername = "username";
		public const string SortByEmail = "email";
		public const string SortByCreated = "created";

		public string Email { get; set; }

		public string Username { get; set; }

		public UserRole[] UserRole { get; set; }

		public bool? EmailVerified { get; set; }

		public class Validator : FilterValidator<GetUsersQuery>
		{
			public Validator()
			{
				RuleFor(m => m.Email).MinLength(1);
				RuleFor(m => m.Username).MinLength(1);
				RuleForEach(m => m.UserRole).IsInEnum();
			}
		}

		public class Handler : FilterQueryHandler<GetUsersQuery, User, UserPreviewDto>
		{
			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<User> repository) : base(mapper, repository)
			{
			}

			/// <inheritdoc />
			protected override void ApplyUserFilter(
				ProjectingSpecification<User, UserPreviewDto> spec, GetUsersQuery request)
			{
				throw new BusinessException("Only admin may query users.");
			}

			/// <inheritdoc />
			protected override void SetupSpecification(GetUsersQuery request,
				ProjectingSpecification<User, UserPreviewDto> spec)
			{
				if (request.Email != null)
				{
					spec.AddCriteria(row
						=> row.Email.ToUpper().StartsWith(request.Email.ToUpper()));
				}

				if (request.Username != null)
				{
					spec.AddCriteria(row
						=> row.UserName.ToUpper().StartsWith(request.Username.ToUpper()));
				}

				if (request.EmailVerified != null)
				{
					spec.AddCriteria(row => row.EmailConfirmed == request.EmailVerified.Value);
				}

				if (request.UserRole != null && request.UserRole.Length > 0)
				{
					spec.AddCriteria(row => request.UserRole.Contains(row.Role));
				}

				spec.Ordered(GetSortingKey(request.SortBy), request.Asc);
			}
			private Expression<Func<User, object>> GetSortingKey(string key)
			{
				switch (key)
				{
					case SortByCreated:
						return row => row.Created;
					case SortByUsername:
						return row => row.UserName;
					case SortByEmail:
						return row => row.Email;
					default:
						return row => row.Id;
				}
			}
		}
	}
}