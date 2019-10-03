using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Specifications;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Games.Commands
{
	public class CreateGameCommand : IRequest<long>, IMapTo<Game>
	{
		public string Name { get; set; }

		public string Key { get; set; }

		public string ImageUrl { get; set; }

		public string DefaultTournamentImageUrl { get; set; }

		public float? DefaultTournamentImageOverlay { get; set; }

		public string DefaultTournamentThemeColor { get; set; }

		public string Description { get; set; }

		public GameType Type { get; set; }

		public long MaxAdditionalFilesSize { get; set; }

		public class Validator : AbstractValidator<CreateGameCommand>
		{
			public Validator()
			{
				RuleFor(m => m.Name).Required().MaxLength(StringLengths.GameName);
				RuleFor(m => m.Key).Required().MaxLength(StringLengths.GameKey);
				RuleFor(m => m.DefaultTournamentImageUrl).Url().MaxLength(StringLengths.Url);
				RuleFor(m => m.ImageUrl).Url().MaxLength(StringLengths.Url);
				RuleFor(m => m.Description).MaxLength(StringLengths.GameDescription);
				RuleFor(m => m.DefaultTournamentImageOverlay).InRange(0, 1);
				RuleFor(m => m.MaxAdditionalFilesSize).MinValue(1);
			}
		}

		public class Handler : IRequestHandler<CreateGameCommand, long>
		{
			private readonly ILogger<CreateGameCommand> logger;
			private readonly IMapper mapper;
			private readonly IRepository<Game> repository;

			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<Game> repository, ILogger<CreateGameCommand> logger)
			{
				this.mapper = mapper;
				this.repository = repository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<long> Handle(CreateGameCommand request, CancellationToken cancellationToken)
			{
				if (await repository.ExistsAsync(g => g.Name == request.Name, cancellationToken))
				{
					throw new ConflictException(
						ValidationErrorCodes.GameNameConflict, nameof(request.Name));
				}

				var game = mapper.Map<Game>(request);
				await repository.CreateAsync(game, cancellationToken);
				logger.GameCreated(game.Id, request);
				return game.Id;
			}
		}
	}
}