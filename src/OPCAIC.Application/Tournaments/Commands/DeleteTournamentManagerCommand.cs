
using FluentValidation;
using MediatR;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Application.Tournaments.Commands
{
	public class DeleteTournamentManagerCommand : IRequest
	{
		public long TournamentId { get; set; }
		public string Email { get; set; }

		public class Validator
			: AbstractValidator<DeleteTournamentManagerCommand>
		{
			public Validator()
			{
				RuleFor(m => m.Email).Email();
			}
		}

		public class Handler : IRequestHandler<DeleteTournamentManagerCommand>
		{
			private readonly IRepository<TournamentManager> managerRepository;
			private readonly IRepository<Tournament> tournamentRepository;

			public Handler(IRepository<TournamentManager> managerRepository,
				IRepository<Tournament> tournamentRepository)
			{
				this.managerRepository = managerRepository;
				this.tournamentRepository = tournamentRepository;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(DeleteTournamentManagerCommand request,
				CancellationToken cancellationToken)
			{
				if (!await tournamentRepository.ExistsAsync(request.TournamentId, cancellationToken))
				{
					throw new NotFoundException(nameof(Tournament), request.TournamentId);
				}
				var deleted = await managerRepository.DeleteAsync(t
						=> t.User.Email == request.Email && t.TournamentId == request.TournamentId,
					cancellationToken);

				if (!deleted)
				{
					throw new UserIsNotManagerOfTournamentException(request.TournamentId,
						request.Email);
				}

				return Unit.Value;
			}
		}
	}
}