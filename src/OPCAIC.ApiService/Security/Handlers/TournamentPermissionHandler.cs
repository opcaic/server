using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using OPCAIC.ApiService.Extensions;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class TournamentPermissionHandler : ResourcePermissionAuthorizationHandler<TournamentPermission>
	{
		private readonly ITournamentRepository tournamentRepository;
		private readonly ITournamentParticipantRepository participantRepository;

		public TournamentPermissionHandler(ITournamentRepository tournamentRepository, ITournamentParticipantRepository participantRepository)
		{
			this.tournamentRepository = tournamentRepository;
			this.participantRepository = participantRepository;
		}

		/// <inheritdoc />
		protected override async Task<bool> HandlePermissionAsync(long userId, ClaimsPrincipal user, TournamentPermission permission,
			long resourceId)
		{
			var authData = await tournamentRepository.GetTournamentAuthorizationData(resourceId);
			switch (permission)
			{
				case TournamentPermission.Create:
					// only organizers
					return user.GetUserRole() == UserRole.Organizer;

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
								await participantRepository.GetParticipantsAsync(resourceId);
							return participants.Any(p => p.User.Id == userId);

						default:
							throw new ArgumentOutOfRangeException();
					}

				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}