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
using OPCAIC.Application.Specifications;
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
			private readonly IRepository<TournamentParticipation> participationsRepository;
			private readonly IUserRepository userRepository;

			public Handler(ITournamentInvitationRepository repository, ITournamentRepository tournamentRepository, IFrontendUrlGenerator urlGenerator, IEmailService emailService, IRepository<TournamentParticipation> participationsRepository, IUserRepository userRepository)
			{
				this.repository = repository;
				this.tournamentRepository = tournamentRepository;
				this.urlGenerator = urlGenerator;
				this.emailService = emailService;
				this.participationsRepository = participationsRepository;
				this.userRepository = userRepository;
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

				var mailDto = new TournamentInvitationEmailDto(
					urlGenerator.TournamentPageLink(request.TournamentId),
					tournamentName
				);

				foreach (var email in toSend)
				{
					// TODO: Fetch all at once 
					var userId =
						await userRepository.FindAsync<User, long?>(u => u.Email == email, u => u.Id, cancellationToken);

					repository.Add(new TournamentInvitation
					{
						TournamentId = request.TournamentId,
						Email = email,
						UserId = userId
					});

					if (userId.HasValue)
					{
						participationsRepository.Add(new TournamentParticipation
						{
							TournamentId = request.TournamentId,
							UserId = userId.Value
						});
					}

					await emailService.EnqueueEmailAsync(mailDto, email, cancellationToken);
				}

				await repository.SaveChangesAsync(cancellationToken);
				await participationsRepository.SaveChangesAsync(cancellationToken);

				return Unit.Value;
			}
		}
	}
}