using AutoMapper;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Emails;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Services
{
	public class TournamentParticipantsService : ITournamentParticipantsService
	{
		private readonly IFrontendUrlGenerator urlGenerator;
		private readonly IEmailService emailService;
		private readonly IMapper mapper;
		private readonly ITournamentRepository tournamentRepository;
		private readonly ITournamentParticipantRepository tournamentParticipantRepository;

		public TournamentParticipantsService(IEmailService emailService, IMapper mapper,
			ITournamentRepository tournamentRepository, ITournamentParticipantRepository tournamentParticipantRepository, IFrontendUrlGenerator urlGenerator)
		{
			this.emailService = emailService;
			this.mapper = mapper;
			this.tournamentRepository = tournamentRepository;
			this.tournamentParticipantRepository = tournamentParticipantRepository;
			this.urlGenerator = urlGenerator;
		}

		public async Task CreateAsync(long tournamentId, IEnumerable<string> emails, CancellationToken cancellationToken)
		{
			if (!await tournamentRepository.ExistsByIdAsync(tournamentId, cancellationToken))
				throw new NotFoundException(nameof(Tournament), tournamentId);

			var participantsDto = await tournamentParticipantRepository.GetParticipantsAsync(tournamentId, null, cancellationToken);

			// add only those addresses, which are not already added
			emails = emails.Where(email => !participantsDto.List.Select(dto => dto.Email).Contains(email));

			await tournamentParticipantRepository.CreateAsync(tournamentId, emails, cancellationToken);

			string tournamentUrl = urlGenerator.TournamentInviteUrl(tournamentId);
			
			foreach (string email in emails)
			{
				await emailService.SendTournamentInvitationEmailAsync(email, tournamentUrl, cancellationToken);
			}
		}

		public async Task DeleteAsync(long tournamentId, string email, CancellationToken cancellationToken)
		{
			if (!await tournamentRepository.ExistsByIdAsync(tournamentId, cancellationToken))
				throw new NotFoundException(nameof(Tournament), tournamentId);

			if (!await tournamentParticipantRepository.DeleteAsync(tournamentId, email, cancellationToken))
				throw new ConflictException(ValidationErrorCodes.UserNotTournamentParticipant, $"User with email '{email}' is not participant of tournament with id {tournamentId}", nameof(email));
		}

		public async Task<ListModel<TournamentParticipantPreviewModel>> GetParticipantsAsync(long tournamentId, TournamentParticipantFilter filter, CancellationToken cancellationToken)
		{
			if (!await tournamentRepository.ExistsByIdAsync(tournamentId, cancellationToken))
				throw new NotFoundException(nameof(Tournament), tournamentId);

			var filterDto = mapper.Map<TournamentParticipantFilterDto>(filter);

			var dtoArray = await tournamentParticipantRepository.GetParticipantsAsync(tournamentId, filterDto, cancellationToken);

			return mapper.Map<ListModel<TournamentParticipantPreviewModel>>(dtoArray);
		}
	}
}
