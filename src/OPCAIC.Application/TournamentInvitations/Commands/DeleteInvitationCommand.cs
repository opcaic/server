using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Extensions;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.TournamentInvitations.Commands
{
	public class DeleteInvitationCommand : IRequest
	{
		/// <inheritdoc />
		public DeleteInvitationCommand(long tournamentId, string email)
		{
			TournamentId = tournamentId;
			Email = email;
		}

		public long TournamentId { get; }

		public string Email { get; }

		public class Validator : AbstractValidator<DeleteInvitationCommand>
		{
			public Validator()
			{
				RuleFor(m => m.Email).Required().Email();
			}
		}

		public class Handler : IRequestHandler<DeleteInvitationCommand>
		{
			private readonly IRepository<TournamentInvitation> repository;
			private readonly IRepository<TournamentParticipation> participationRepository;

			public Handler(IRepository<TournamentInvitation> repository, IRepository<TournamentParticipation> participationRepository)
			{
				this.repository = repository;
				this.participationRepository = participationRepository;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(DeleteInvitationCommand request, CancellationToken cancellationToken)
			{
				var invitation = await repository.GetAsync(i => i.TournamentId == request.TournamentId && i.Email == request.Email, cancellationToken);

				if (invitation.UserId != null) // user exists, delete him if he has not already submitted something
				{
					await participationRepository.DeleteAsync(p
						=> p.TournamentId == request.TournamentId &&
						p.UserId == invitation.UserId &&
						!p.Submissions.Any(), cancellationToken);
				}

				return Unit.Value;
			}
		}
	}
}