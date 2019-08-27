using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class MatchExecutionPermissionHandler
		: ResourcePermissionAuthorizationHandler<MatchExecutionPermission, EmptyAuthData>
	{
		private readonly IMatchRepository matchRepository;

		public MatchExecutionPermissionHandler(IMatchRepository matchRepository)
		{
			this.matchRepository = matchRepository;
		}

		/// <inheritdoc />
		protected override async Task<EmptyAuthData> GetAuthorizationData(long resourceId,
			CancellationToken cancellationToken = default)
		{
			return EmptyAuthData.Instance;
		}

		/// <inheritdoc />
		protected override bool HandlePermissionAsync(long userId, ClaimsPrincipal user,
			MatchExecutionPermission permission,
			EmptyAuthData _)
		{
			switch (permission)
			{
				case MatchExecutionPermission.UploadResult:
					return false; // TODO: worker authorization

				case MatchExecutionPermission.DownloadResults:
					return false; // TODO:

				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}