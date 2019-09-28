using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.Domain.ValueObjects;

namespace OPCAIC.Application.Tournaments.Commands
{
	public class UpdateTournamentCommand : IIdentifiedRequest, IMapTo<Tournament>, IRequest
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public TournamentFormat Format { get; set; }

		public TournamentScope Scope { get; set; }

		public TournamentRankingStrategy RankingStrategy { get; set; }

		public int? MatchesPerDay { get; set; }

		public long MaxSubmissionSize { get; set; }

		public bool PrivateMatchLog { get; set; }

		public List<MenuItemDto> MenuItems { get; set; }

		/// <inheritdoc />
		public long Id { get; set; }

		public class Validator : AbstractValidator<UpdateTournamentCommand>
		{
			public class MenuItemValidator : AbstractValidator<MenuItemDto>
			{
				public MenuItemValidator()
				{
					RuleFor(m => m.Type).Required().IsInEnum();

					When(m => m.Type == MenuItemType.DocumentLink, () =>
					{
						RuleFor(m => ((DocumentLinkMenuItemDto)m).DocumentId)
							.EntityId(typeof(Document));
					});

					When(m => m.Type == MenuItemType.ExternalUrl, () =>
					{
						RuleFor(m => ((ExternalUrlMenuItemDto)m).ExternalLink)
							.Url();
						RuleFor(m => ((ExternalUrlMenuItemDto)m).Text)
							.MaxLength(StringLengths.MenuItemText);
					});
				}
			}

			public Validator(MenuItemValidator menuItemValidator)
			{
				RuleFor(m => m.Name).Required().MaxLength(StringLengths.TournamentName);

				RuleForEach(m => m.MenuItems).SetValidator(menuItemValidator);

				// keep these rules synchronized with NewTournamentModel
				RuleFor(m => m.Format).IsInEnum().NotEqual(TournamentFormat.Unknown)
					.NotEqual(TournamentFormat.Elo).When(m => m.Scope == TournamentScope.Deadline)
					.WithMessage("Deadline tournaments do not support ELO format.")
					.Equal(TournamentFormat.Elo).When(m => m.Scope == TournamentScope.Ongoing)
					.WithMessage("Only ELO format is supported for ongoing tournaments");

				RuleFor(m => m.Scope).IsInEnum().NotEqual(TournamentScope.Unknown);
				RuleFor(m => m.RankingStrategy).IsInEnum().NotEqual(TournamentRankingStrategy.Unknown);

				RuleFor(m => m.MatchesPerDay)
					.Null().When(m => m.Scope == TournamentScope.Deadline)
					.NotNull().When(m => m.Scope == TournamentScope.Ongoing);

				RuleFor(m => m.MaxSubmissionSize).MinValue(1);
			}
		}

		public class Handler : IRequestHandler<UpdateTournamentCommand>
		{
			private readonly IMapper mapper;

			/// <inheritdoc />
			public Handler(IMapper mapper, ITournamentRepository repository, ILogger<UpdateTournamentCommand> logger)
			{
				this.mapper = mapper;
				this.repository = repository;
				this.logger = logger;
			}

			private readonly ITournamentRepository repository;
			private readonly ILogger<UpdateTournamentCommand> logger;

			/// <inheritdoc />
			public async Task<Unit> Handle(UpdateTournamentCommand request, CancellationToken cancellationToken)
			{
				if (!await repository.UpdateAsync(request.Id, request, cancellationToken))
				{
					throw new NotFoundException(nameof(Tournament), request.Id);
				}

				logger.TournamentUpdated(request.Id, request);
				return Unit.Value;
			}
		}
	}
}