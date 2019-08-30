using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Extensions;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class TournamentPermissionHandler
		: ResourcePermissionAuthorizationHandler<TournamentPermission, TournamentAuthDto>
	{
		private readonly ITournamentParticipantRepository participantRepository;
		private readonly ITournamentRepository tournamentRepository;

		public TournamentPermissionHandler(ITournamentRepository tournamentRepository,
			ITournamentParticipantRepository participantRepository)
		{
			this.tournamentRepository = tournamentRepository;
			this.participantRepository = participantRepository;
		}

		/// <inheritdoc />
		protected override Task<TournamentAuthDto> GetAuthorizationData(long resourceId,
			CancellationToken cancellationToken = default)
		{
			return tournamentRepository.GetAuthorizationData(resourceId, cancellationToken);
		}

		/// <inheritdoc />
		protected override bool HandlePermissionAsync(long userId, ClaimsPrincipal user,
			TournamentPermission permission,
			TournamentAuthDto authData)
		{
			switch (permission)
			{
				case TournamentPermission.Create:
					// only organizers
					return user.GetUserRole() == UserRole.Organizer;

				case TournamentPermission.ManageInvites:
				case TournamentPermission.EditDocument:
				case TournamentPermission.Update:
					// only owner and managers
					return userId == authData.OwnerId ||
						authData.ManagerIds.Contains(userId);

				case TournamentPermission.Delete:
					// only owner
					return userId == authData.OwnerId;

				case TournamentPermission.Read:
				case TournamentPermission.Search:
					// everyone
					return true; // TODO: verify this

				case TournamentPermission.Submit:
				case TournamentPermission.Join:
					switch (authData.Availability)
					{
						case TournamentAvailability.Public:
							// everyone
							return true;

						case TournamentAvailability.Private:
							// only invited
							var participants =
								participantRepository.GetParticipantsAsync(authData.Id, null)
                .Result;
							return participants.List.Any(p => p.User.Id == userId);

						default:
							throw new ArgumentOutOfRangeException();
					}

				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}