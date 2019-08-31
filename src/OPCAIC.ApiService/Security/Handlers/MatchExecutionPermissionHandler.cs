using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class MatchExecutionPermissionHandler
		: ResourcePermissionAuthorizationHandler<MatchExecutionPermission, ResourceIdAuth>
	{
		private readonly IMatchRepository matchRepository;

		public MatchExecutionPermissionHandler(IMatchRepository matchRepository)
		{
			this.matchRepository = matchRepository;
		}

		/// <inheritdoc />
		protected override Task<ResourceIdAuth> GetAuthorizationData(long resourceId,
			CancellationToken cancellationToken = default)
		{
			return Task.FromResult(new ResourceIdAuth(resourceId));
		}

		/// <inheritdoc />
		protected override bool HandlePermissionAsync(ClaimsPrincipal user,
			MatchExecutionPermission permission,
			ResourceIdAuth authData)
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