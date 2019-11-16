using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.Domain.ValueObjects;

namespace OPCAIC.Application.Tournaments.Commands
{
	public class CloneTournamentCommand : AuthenticatedRequest, IRequest<long>
	{
		public CloneTournamentCommand(long id)
		{
			Id = id;
		}

		public long Id { get; }

		public class Handler : IRequestHandler<CloneTournamentCommand, long>
		{
			private readonly ILogger<CloneTournamentCommand> logger;
			private readonly IRepository<Tournament> repository;

			public Handler(IRepository<Tournament> repository, ILogger<CloneTournamentCommand> logger)
			{
				this.repository = repository;
				this.logger = logger;
			}

			/// <inheritdoc />
			public async Task<long> Handle(CloneTournamentCommand request, CancellationToken cancellationToken)
			{
				var spec = new BaseSpecification<Tournament>()
					.AddCriteria(t => t.Id == request.Id)
					.Include($"{nameof(Tournament.MenuItems)}.{nameof(DocumentLinkMenuItem.Document)}")
					.Include(nameof(Tournament.Documents))
					.AsReadOnly(); // disables change tracking in repository

				// does not fetch related entities aside from explicitly included ones
				var tournament = await repository.FindAsync(spec, cancellationToken) ??
					throw new NotFoundException(nameof(Tournament), request.Id);

				tournament.OwnerId = request.RequestingUserId;

				// clear data tournament data
				tournament.State = TournamentState.Created;
				tournament.Published = null;
				tournament.EvaluationStarted = null;
				tournament.EvaluationFinished = null;

				// take care of duplicated objects
				var documents = tournament.Documents.ToDictionary(d => d.Id);

				// clear Ids, since the objects are not tracked by repository, new rows in database
				// are created
				tournament.Id = 0;
				foreach (var item in tournament.Documents)
				{
					item.Id = 0;
				}
				foreach (var item in tournament.MenuItems.OfType<DocumentLinkMenuItem>())
				{
					item.Document = documents.GetValueOrDefault(item.DocumentId);
					item.DocumentId = 0;
				}

				await repository.CreateAsync(tournament, cancellationToken);

				logger.TournamentCreated(tournament.Id, tournament.Name, tournament.GameId);
				return tournament.Id;
			}
		}
	}
}
