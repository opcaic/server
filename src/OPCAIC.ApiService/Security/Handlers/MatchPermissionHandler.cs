using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Extensions;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class MatchPermissionHandler
		: ResourcePermissionAuthorizationHandler<MatchPermission, MatchAuthDto>
	{
		private readonly IMatchRepository matchRepository;

		public MatchPermissionHandler(IMatchRepository matchRepository)
		{
			this.matchRepository = matchRepository;
		}

		/// <inheritdoc />
		protected override Task<MatchAuthDto> GetAuthorizationData(long resourceId,
			CancellationToken cancellationToken = default)
		{
			return matchRepository.GetAuthorizationData(resourceId, cancellationToken);
		}

		/// <inheritdoc />
		protected override bool HandlePermissionAsync(ClaimsPrincipal user,
			MatchPermission permission,
			MatchAuthDto authData)
		{
			switch (permission)
			{
				case MatchPermission.Read:
					// authors of participated submissions and tournament managers
					var userId = user.TryGetId();
					return authData.ParticipantsIds.Contains(userId) ||
						authData.TournamentManagersIds.Contains(userId) ||
						authData.TournamentOwnerId == userId;

				case MatchPermission.QueueMatchExecution:
					return false; // only admin //TODO: really?

				case MatchPermission.Search:
					return true; // TODO: maybe more granular.

				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}