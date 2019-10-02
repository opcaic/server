using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enumerations;

namespace OPCAIC.Application.Users.Commands
{
	public class ChangeLanguageCommand : AuthenticatedRequest, IRequest
	{
		public string Language { get; set; }

		public class Validator : AbstractValidator<ChangeLanguageCommand>
		{
			public Validator()
			{
				RuleFor(m => m.Language)
					.IsEnumeration<ChangeLanguageCommand, LocalizationLanguage>().Required();
			}
		}

		public class Handler : IRequestHandler<ChangeLanguageCommand>
		{
			private readonly ILogger<ChangeLanguageCommand> logger;
			private readonly IRepository<User> repository;

			public Handler(IRepository<User> repository, ILogger<ChangeLanguageCommand> logger)
			{
				this.repository = repository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(ChangeLanguageCommand request, CancellationToken cancellationToken)
			{
				var dto = new UpdateDto(request.Language);
				await repository.UpdateAsync(request.RequestingUserId,
					dto, cancellationToken);

				logger.UserUpdated(request.RequestingUserId, dto);
				return Unit.Value;
			}

			public class UpdateDto : IMapTo<User>
			{
				public UpdateDto(string language)
				{
					LocalizationLanguage = language;
				}

				public string LocalizationLanguage { get; }
			}
		}
	}
}