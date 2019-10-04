using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Tournaments.Commands
{
	public class CreateTournamentCommand : AuthenticatedRequest, IMapTo<Tournament>, IRequest<long>
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public JObject Configuration { get; set; }

		public long GameId { get; set; }

		public TournamentFormat Format { get; set; }

		public TournamentScope Scope { get; set; }

		public TournamentRankingStrategy RankingStrategy { get; set; }

		public int? MatchesPerDay { get; set; }

		public long MaxSubmissionSize { get; set; }

		public List<MenuItemDto> MenuItems { get; set; }

		public string ImageUrl { get; set; }

		public double? ImageOverlay { get; set; }

		public string ThemeColor { get; set; }

		public DateTime? Deadline { get; set; }

		public TournamentAvailability Availability { get; set; }

		public bool PrivateMatchlog { get; set; }

		public class Validator : AbstractValidator<CreateTournamentCommand>
		{
			public Validator()
			{
				RuleFor(m => m.Name).Required().MaxLength(StringLengths.TournamentName);
				RuleFor(m => m.GameId).EntityId(typeof(Game))
					.DependentRules(() =>
					{
						RuleFor(m => m.Configuration).ValidGameConfiguration(g => g.GameId);
					});

				// keep these rules synchronized with UpdateTournamentCommand
				RuleFor(m => m.Format).IsInEnum().NotEqual(TournamentFormat.Unknown)
					.NotEqual(TournamentFormat.Elo).When(m => m.Scope == TournamentScope.Deadline)
					.WithMessage("Deadline tournaments do not support ELO format.")
					.Equal(TournamentFormat.Elo).When(m => m.Scope == TournamentScope.Ongoing)
					.WithMessage("Only ELO format is supported for ongoing tournaments");

				RuleFor(m => m.Scope).IsInEnum().NotEqual(TournamentScope.Unknown);
				RuleFor(m => m.RankingStrategy).IsInEnum()
					.NotEqual(TournamentRankingStrategy.Unknown);

				RuleFor(m => m.MatchesPerDay)
					.Null().When(m => m.Scope == TournamentScope.Deadline)
					.NotNull().When(m => m.Scope == TournamentScope.Ongoing);

				RuleFor(m => m.MaxSubmissionSize).MinValue(1);

				RuleFor(m => m.ImageUrl).Url().MaxLength(StringLengths.Url);
				RuleFor(m => m.ImageOverlay).InRange(0, 1);
				RuleFor(m => m.ThemeColor).MaxLength(StringLengths.ThemeColor);

				RuleFor(m => m.Deadline)
					.NotNull().GreaterThan(DateTime.Now).When(m => m.Scope == TournamentScope.Deadline)
					.Null().When(m => m.Scope == TournamentScope.Ongoing);

				RuleFor(m => m.Description).MaxLength(StringLengths.GameDescription);
			}
		}

		public class Handler : IRequestHandler<CreateTournamentCommand, long>
		{
			private readonly ILogger<CreateTournamentCommand> logger;
			private readonly IMapper mapper;
			private readonly IRepository<Tournament> repository;

			public Handler(IMapper mapper, ILogger<CreateTournamentCommand> logger,
				IRepository<Tournament> repository)
			{
				this.mapper = mapper;
				this.logger = logger;
				this.repository = repository;
			}

			/// <inheritdoc />
			public async Task<long> Handle(CreateTournamentCommand request,
				CancellationToken cancellationToken)
			{
				var tournament = mapper.Map<Tournament>(request);
				tournament.OwnerId = request.RequestingUserId;

				await repository.CreateAsync(tournament, cancellationToken);
				logger.TournamentCreated(tournament.Id, request);
				return tournament.Id;
			}
		}
	}
}