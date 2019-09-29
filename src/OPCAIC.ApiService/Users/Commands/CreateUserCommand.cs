using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.Users.Events;
using OPCAIC.Application.Dtos.EmailTemplates;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.ApiService.Users.Commands
{
	public class CreateUserCommand : IRequest<long>, IMapTo<User>
	{
		public string Email { get; set; }

		public string Username { get; set; }

		public string Organization { get; set; }

		public string LocalizationLanguage { get; set; }

		[IgnoreMap]
		public string Password { get; set; }

		public bool WantsEmailNotifications { get; set; }

		public class Validator : AbstractValidator<CreateUserCommand>
		{
			public Validator()
			{
				RuleFor(m => m.Email).Email().Required();
				RuleFor(m => m.Username).Required();
				RuleFor(m => m.Organization).MinLength(1);

				// TODO: choice from available localizations
				RuleFor(m => m.LocalizationLanguage).Required().MinLength(2).MaxLength(2);

				RuleFor(m => m.Password).Required();
			}
		}

		public class Handler : IRequestHandler<CreateUserCommand, long>
		{
			private readonly IMediator mediator;
			private readonly IMapper mapper;
			private readonly IUserManager userManager;
			private readonly ILogger<CreateUserCommand> logger;

			/// <inheritdoc />
			public Handler(IMapper mapper, IUserManager userManager, ILogger<CreateUserCommand> logger, IMediator mediator)
			{
				this.mapper = mapper;
				this.userManager = userManager;
				this.logger = logger;
				this.mediator = mediator;
			}

			/// <inheritdoc />
			public async Task<long> Handle(CreateUserCommand request, CancellationToken cancellationToken)
			{
				var user = mapper.Map<User>(request);
				user.Role = UserRole.User;

				var result = await userManager.CreateAsync(user, request.Password);
				result.ThrowIfFailed();

				logger.UserCreated(user);
				await mediator.Publish(new UserCreated(user), cancellationToken);

				return user.Id;
			}
		}
	}
}