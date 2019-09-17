using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Extensions;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Enums;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class TournamentPermissionHandler
		: ResourcePermissionAuthorizationHandler<TournamentPermission, TournamentAuthDto>
	{
		private readonly ITournamentRepository tournamentRepository;

		public TournamentPermissionHandler(ITournamentRepository tournamentRepository)
		{
			this.tournamentRepository = tournamentRepository;
		}

		/// <inheritdoc />
		protected override Task<TournamentAuthDto> GetAuthorizationData(long resourceId,
			CancellationToken cancellationToken = default)
		{
			return tournamentRepository.GetAuthorizationData(resourceId, cancellationToken);
		}

		/// <inheritdoc />
		protected override bool HandlePermissionAsync(ClaimsPrincipal user,
			TournamentPermission permission,
			TournamentAuthDto authData)
		{
			var userId = user.TryGetId();

			switch (permission)
			{
				case TournamentPermission.Create:
					// only organizers
					return user.GetUserRole() == UserRole.Organizer;

				case TournamentPermission.UploadAdditionalFiles:
				case TournamentPermission.StartTournamentEvaluation:
				case TournamentPermission.ManageInvites:
				case TournamentPermission.EditDocument:
				case TournamentPermission.Update:
					// only owner and managers
					return IsOwnerOrManager(userId, authData);

				case TournamentPermission.Delete:
					// only owner
					return userId == authData.OwnerId;

				case TournamentPermission.Read:
				case TournamentPermission.Search:
					// everyone
					return true; // TODO: verify this

				case TournamentPermission.Submit:
				case TournamentPermission.Join:
				case TournamentPermission.ViewLeaderboard:
					switch (authData.Availability)
					{
						case TournamentAvailability.Public:
							// everyone
							return true;

						case TournamentAvailability.Private:
							// only invited people and managers
							return authData.ParticipantIds.Contains(userId) ||
								IsOwnerOrManager(userId, authData);

						default:
							throw new ArgumentOutOfRangeException();
					}
				case TournamentPermission.DownloadAdditionalFiles:
					return
						user.HasClaim(WorkerClaimTypes.TournamentId, authData.Id.ToString()) ||
						IsOwnerOrManager(userId, authData);

				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}

		private bool IsOwnerOrManager(long userId, TournamentAuthDto authData)
		{
			return userId == authData.OwnerId || authData.ManagerIds.Contains(userId);
		}
	}
}