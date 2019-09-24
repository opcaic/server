using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using OPCAIC.Application.Dtos.EmailTemplates;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.TournamentInvitations.Commands
{
	public class InvitePlayersCommand : IRequest
	{
		public long TournamentId { get; set; }
		public string[] Emails { get; set; }

		public class Validator
			: AbstractValidator<InvitePlayersCommand>
		{
			public Validator()
			{
				RuleFor(m => m.Emails).ForEach(f => f.Email());
			}
		}

		public class Handler : IRequestHandler<InvitePlayersCommand>
		{
			private readonly IEmailService emailService;
			private readonly IFrontendUrlGenerator urlGenerator;
			private readonly ITournamentRepository tournamentRepository;
			private readonly ITournamentInvitationRepository repository;

			public Handler(ITournamentInvitationRepository repository, ITournamentRepository tournamentRepository, IFrontendUrlGenerator urlGenerator, IEmailService emailService)
			{
				this.repository = repository;
				this.tournamentRepository = tournamentRepository;
				this.urlGenerator = urlGenerator;
				this.emailService = emailService;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(InvitePlayersCommand request, CancellationToken cancellationToken)
			{
				var tournamentName =
					await tournamentRepository.GetAsync(request.TournamentId,
						t => t.Name, cancellationToken);

				var invitations = await repository.ListAsync(
					i => i.TournamentId == request.TournamentId,
					i => i.Email,
					cancellationToken);

				// add only those addresses, which are not already added
				var toSend = request.Emails.Where(invite => !invitations.Contains(invite)).ToList();

				await repository.CreateAsync(request.TournamentId, toSend,
					cancellationToken);

				var mailDto = new TournamentInvitationEmailDto
				{
					TournamentUrl = urlGenerator.TournamentPageLink(request.TournamentId),
					TournamentName = tournamentName
				};

				// todo: make TournamentUserInvited event + handler?
				foreach (string email in toSend)
				{
					await emailService.EnqueueEmailAsync(mailDto, email, cancellationToken);
				}

				return Unit.Value;
			}
		}
	}
}