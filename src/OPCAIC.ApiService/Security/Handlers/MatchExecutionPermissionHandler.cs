using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.Dtos.MatchExecutions;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class MatchExecutionPermissionHandler
		: ResourcePermissionAuthorizationHandler<MatchExecutionPermission, MatchExecutionAuthDto>
	{
		private readonly IMatchExecutionRepository repository;

		public MatchExecutionPermissionHandler(IMatchExecutionRepository repository)
		{
			this.repository = repository;
		}

		/// <inheritdoc />
		protected override Task<MatchExecutionAuthDto> GetAuthorizationData(long resourceId,
			CancellationToken cancellationToken = default)
		{
			return repository.GetAuthorizationData(resourceId, cancellationToken);
		}

		/// <inheritdoc />
		protected override bool HandlePermissionAsync(ClaimsPrincipal user,
			MatchExecutionPermission permission,
			MatchExecutionAuthDto authData)
		{
			switch (permission)
			{
				case MatchExecutionPermission.UploadResult:
					return user.HasClaim(WorkerClaimTypes.ExecutionId, authData.Id.ToString());

				case MatchExecutionPermission.ReadDetail:
				case MatchExecutionPermission.DownloadResults:
					var userId = user.TryGetId();
					return authData.TournamentOwnerId == userId ||
						authData.MatchParticipantsUserIds.Contains(userId) ||
						authData.TournamentManagersIds.Contains(userId);

				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}