using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.Application.Dtos.EmailTemplates;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Exceptions;

namespace OPCAIC.ApiService.Services
{
	public class TournamentInvitationsService : ITournamentInvitationsService
	{
		private readonly IEmailService emailService;
		private readonly IMapper mapper;
		private readonly ITournamentInvitationRepository tournamentInvitationRepository;
		private readonly ITournamentRepository tournamentRepository;
		private readonly IFrontendUrlGenerator urlGenerator;

		public TournamentInvitationsService(IEmailService emailService, IMapper mapper,
			ITournamentRepository tournamentRepository,
			ITournamentInvitationRepository tournamentInvitationRepository,
			IFrontendUrlGenerator urlGenerator)
		{
			this.emailService = emailService;
			this.mapper = mapper;
			this.tournamentRepository = tournamentRepository;
			this.tournamentInvitationRepository = tournamentInvitationRepository;
			this.urlGenerator = urlGenerator;
		}

		public async Task CreateAsync(long tournamentId, IEnumerable<string> emails,
			CancellationToken cancellationToken)
		{
			var tournament =
				await tournamentRepository.FindByIdAsync(tournamentId, cancellationToken);
			if (tournament == null)
			{
				throw new NotFoundException(nameof(Tournament), tournamentId);
			}

			var participantsDto =
				await tournamentInvitationRepository.GetInvitationsAsync(tournamentId, null,
					cancellationToken);

			// add only those addresses, which are not already added
			emails = emails.Where(email
				=> !participantsDto.List.Select(dto => dto.Email).Contains(email));

			await tournamentInvitationRepository.CreateAsync(tournamentId, emails,
				cancellationToken);

			string tournamentUrl = urlGenerator.TournamentPageLink(tournamentId);

			var mailDto = new TournamentInvitationEmailDto
			{
				TournamentUrl = tournamentUrl, TournamentName = tournament.Name
			};

			foreach (string email in emails)
			{
				await emailService.EnqueueEmailAsync(mailDto, email, cancellationToken);
			}
		}

		public async Task DeleteAsync(long tournamentId, string email,
			CancellationToken cancellationToken)
		{
			if (!await tournamentRepository.ExistsByIdAsync(tournamentId, cancellationToken))
				throw new NotFoundException(nameof(Tournament), tournamentId);

			if (!await tournamentInvitationRepository.DeleteAsync(tournamentId, email,
				cancellationToken))
				throw new ConflictException(ValidationErrorCodes.UserNotInvited,
					$"User with email '{email}' is not participant of tournament with id {tournamentId}",
					nameof(email));
		}

		public async Task<ListModel<TournamentInvitationPreviewModel>> GetInvitationsAsync(
			long tournamentId, TournamentInvitationFilter filter,
			CancellationToken cancellationToken)
		{
			if (!await tournamentRepository.ExistsByIdAsync(tournamentId, cancellationToken))
				throw new NotFoundException(nameof(Tournament), tournamentId);

			var filterDto = mapper.Map<TournamentInvitationFilterDto>(filter);

			var dtoArray =
				await tournamentInvitationRepository.GetInvitationsAsync(tournamentId, filterDto,
					cancellationToken);

			return mapper.Map<ListModel<TournamentInvitationPreviewModel>>(dtoArray);
		}
	}
}