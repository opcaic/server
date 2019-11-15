using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Games.Commands
{
	public class DeleteGameCommand : IRequest
	{
		public DeleteGameCommand(in long id)
		{
			GameId = id;
		}

		public long GameId { get; }
		
		public class Handler : IRequestHandler<DeleteGameCommand>
		{
			private readonly IRepository<Game> repository;
			private readonly IMediator mediator;

			public Handler(IRepository<Game> repository, IMediator mediator)
			{
				this.repository = repository;
				this.mediator = mediator;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(DeleteGameCommand request, CancellationToken cancellationToken)
			{
				var tournaments = await repository.FindAsync(request.GameId, g => g.Tournaments.Select(s => s.Id).ToList(), cancellationToken);

				foreach (var tournament in tournaments)
				{
					await mediator.Send(new DeleteTournamentCommand(tournament), cancellationToken);
				}

				await repository.DeleteAsync(request.GameId, cancellationToken);
				return Unit.Value;
			}
		}
	}
}