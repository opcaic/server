using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Specifications;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enumerations;
using OPCAIC.Domain.Enums;

namespace OPCAIC.ApiService.Users.Commands
{
	public class UpdateUserCommand
		: AuthenticatedRequest, IIdentifiedRequest, IRequest, IMapTo<User>
	{
		public string Organization { get; set; }

		public LocalizationLanguage LocalizationLanguage { get; set; }

		public UserRole Role { get; set; }

		public bool WantsEmailNotifications { get; set; }

		/// <inheritdoc />
		public long Id { get; set; }

		public class Validator : AbstractValidator<UpdateUserCommand>
		{
			public Validator()
			{
				RuleFor(m => m.Organization).MaxLength(StringLengths.Organization);
				RuleFor(m => m.LocalizationLanguage).Required();
				RuleFor(m => m.Role).Required();
			}
		}

		public class Handler : IRequestHandler<UpdateUserCommand>
		{
			private readonly IRepository<User> repository;

			/// <inheritdoc />
			public Handler(IRepository<User> repository)
			{
				this.repository = repository;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(UpdateUserCommand request,
				CancellationToken cancellationToken)
			{
				if (request.Role != request.RequestingUserRole)
				{
					if (request.RequestingUserRole != UserRole.Admin)
					{
						throw new BusinessException("Only admin can change user roles.");
					}

					// changing from admin role to non-admin, make sure there is at least one other admin to prevent platform lockout
					if (request.RequestingUserId == request.Id &&
						!await repository.ExistsAsync(u
							=> u.Role == UserRole.Admin && u.Id != request.Id, cancellationToken))
					{
						throw new BusinessException(
							"Cannot change role of the last admin user.");
					}
				}

				await repository.UpdateAsync(u => u.Id == request.Id, request, cancellationToken);

				return Unit.Value;
			}
		}
	}
}