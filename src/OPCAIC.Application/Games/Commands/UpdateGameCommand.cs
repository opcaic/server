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
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Games.Commands
{
	public class UpdateGameCommand : IRequest, IIdentifiedRequest, IMapTo<Game>
	{
		/// <inheritdoc />
		public long Id { get; set; }
		
		public string Name { get; set; }

		public string Key { get; set; }

		public JObject ConfigurationSchema { get; set; }

		public string ImageUrl { get; set; }

		public string DefaultTournamentImageUrl { get; set; }

		public float? DefaultTournamentImageOverlay { get; set; }

		public string DefaultTournamentThemeColor { get; set; }

		public string Description { get; set; }

		public GameType Type { get; set; }

		public long MaxAdditionalFilesSize { get; set; }

		public class Validator : AbstractValidator<UpdateGameCommand>
		{
			public Validator()
			{
				RuleFor(m => m.Name).Required().MaxLength(StringLengths.GameName);
				RuleFor(m => m.Key).Required().MaxLength(StringLengths.GameKey);
				RuleFor(m => m.DefaultTournamentImageUrl).MaxLength(StringLengths.Url);
				RuleFor(m => m.ImageUrl).MaxLength(StringLengths.Url);
				RuleFor(m => m.Description).MaxLength(StringLengths.GameDescription);
				RuleFor(m => m.DefaultTournamentImageOverlay).InclusiveBetween(0, 1);
				RuleFor(m => m.ConfigurationSchema).ValidSchema();
				RuleFor(m => m.MaxAdditionalFilesSize).MinValue(1);
			}
		}

		public class Handler : IRequestHandler<UpdateGameCommand>
		{
			private readonly IMapper mapper;
			private readonly IRepository<Game> repository;
			private readonly ILogger<UpdateGameCommand> logger;

			/// <inheritdoc />
			public Handler(IMapper mapper, IRepository<Game> repository, ILogger<UpdateGameCommand> logger)
			{
				this.mapper = mapper;
				this.repository = repository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(UpdateGameCommand request, CancellationToken cancellationToken)
			{
				// check if other game of same name already exists
				if (await repository.ExistsAsync(g => g.Name == request.Name && g.Id != request.Id, cancellationToken))
				{
					throw new ConflictException(
						ValidationErrorCodes.GameNameConflict, nameof(request.Name));
				}

				await repository.UpdateAsync(request.Id, request, cancellationToken);
				logger.GameUpdated(request.Id, request);

				return Unit.Value;
			}
		}
	}
}