using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Specifications;
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

		public JObject Configuration { get; set; }

		public TournamentFormat Format { get; set; }

		public TournamentScope Scope { get; set; }

		public TournamentRankingStrategy RankingStrategy { get; set; }

		public int? MatchesPerDay { get; set; }

		public long MaxSubmissionSize { get; set; }

		public bool PrivateMatchLog { get; set; }

		public List<MenuItemDto> MenuItems { get; set; }

		public string ImageUrl { get; set; }

		public double? ImageOverlay { get; set; }

		public string ThemeColor { get; set; }

		public DateTime? Deadline { get; set; }

		public TournamentAvailability Availability { get; set; }

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

			public Validator(MenuItemValidator menuItemValidator, ITimeService time)
			{
				RuleFor(m => m.Name).Required().MaxLength(StringLengths.TournamentName);

				RuleForEach(m => m.MenuItems).SetValidator(menuItemValidator);

				RuleFor(m => m.Configuration).Required().ValidGameConfigurationViaTournamentId(m => m.Id);

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

				RuleFor(m => m.ImageUrl).Url().MaxLength(StringLengths.Url);
				RuleFor(m => m.ImageOverlay).InRange(0, 1);
				RuleFor(m => m.ThemeColor).MaxLength(StringLengths.ThemeColor);

				RuleFor(m => m.Deadline)
					.NotNull().GreaterThan(time.Now).When(m => m.Scope == TournamentScope.Deadline)
					.Null().When(m => m.Scope == TournamentScope.Ongoing);

				RuleFor(m => m.Description).MaxLength(StringLengths.GameDescription);
			}
		}

		public class Handler : IRequestHandler<UpdateTournamentCommand>
		{
			private readonly ILogger<UpdateTournamentCommand> logger;
			private readonly IMapper mapper;
			private readonly IRepository<Tournament> repository;

			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<Tournament> repository,
				ILogger<UpdateTournamentCommand> logger)
			{
				this.mapper = mapper;
				this.repository = repository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(UpdateTournamentCommand request,
				CancellationToken cancellationToken)
			{
				var tournament = await repository.GetAsync(request.Id, cancellationToken);

				if (tournament.State != TournamentState.Created)
				{
					if (request.Format != tournament.Format)
					{
						throw new PublishedTournamentUpdateException(request.Id,
							nameof(Tournament.Format));
					}

					if (request.Scope != tournament.Scope)
					{
						throw new PublishedTournamentUpdateException(request.Id,
							nameof(Tournament.Scope));
					}

					if (request.RankingStrategy != tournament.RankingStrategy)
					{
						throw new PublishedTournamentUpdateException(request.Id,
							nameof(Tournament.RankingStrategy));
					}
				}

				await repository.UpdateAsync(request.Id, request, cancellationToken);

				logger.TournamentUpdated(request.Id, request);
				return Unit.Value;
			}
		}
	}
}