using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces.Repositories;

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
			private readonly ITournamentInvitationRepository repository;

			public Handler(ITournamentInvitationRepository repository)
			{
				this.repository = repository;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(DeleteInvitationCommand request, CancellationToken cancellationToken)
			{
				await repository.DeleteAsync(request.TournamentId, request.Email, cancellationToken);

				return Unit.Value;
			}
		}
	}
}