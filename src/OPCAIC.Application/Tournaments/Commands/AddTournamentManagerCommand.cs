using FluentValidation;
using MediatR;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Application.Tournaments.Command
{
	public class AddTournamentManagerCommand : IRequest
	{
		public long TournamentId { get; set; }
		public string Email { get; set; }

		public class Validator
			: AbstractValidator<AddTournamentManagerCommand>
		{
			public Validator()
			{
				RuleFor(m => m.Email).Email();
			}
		}

		public class Handler : IRequestHandler<AddTournamentManagerCommand>
		{
			private readonly IRepository<Tournament> tournamentRepository;
			private readonly IRepository<User> userRepository;
			private readonly IRepository<TournamentManager> managerRepository;

			public Handler(IRepository<Tournament> tournamentRepository, IRepository<User> userRepository,
				IRepository<TournamentManager> managerRepository)
			{
				this.tournamentRepository = tournamentRepository;
				this.userRepository = userRepository;
				this.managerRepository = managerRepository;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(AddTournamentManagerCommand request, CancellationToken cancellationToken)
			{
				var tournament = await tournamentRepository.GetAsync(request.TournamentId, cancellationToken);
				var user = await userRepository.GetAsync(u => u.Email == request.Email, cancellationToken);

				if (await managerRepository.ExistsAsync(t => t.TournamentId == request.TournamentId && t.User.Email == user.Email, cancellationToken))
				{
					throw new UserIsAlreadyManagerOfTournamentException(request.TournamentId, user.Email);
				}

				managerRepository.Add(new TournamentManager { TournamentId = tournament.Id, UserId = user.Id });

				return Unit.Value;
			}
		}
	}
}
