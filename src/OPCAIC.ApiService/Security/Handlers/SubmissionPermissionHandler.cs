using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos.Submissions;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class SubmissionPermissionHandler
		: ResourcePermissionAuthorizationHandler<SubmissionPermission, SubmissionAuthDto>
	{
		private readonly ISubmissionRepository submissionRepository;

		public SubmissionPermissionHandler(ISubmissionRepository submissionRepository)
		{
			this.submissionRepository = submissionRepository;
		}

		/// <inheritdoc />
		protected override Task<SubmissionAuthDto> GetAuthorizationData(long resourceId,
			CancellationToken cancellationToken = default)
		{
			return submissionRepository.GetAuthorizationData(resourceId, cancellationToken);
		}

		/// <inheritdoc />
		protected override bool HandlePermissionAsync(long userId, ClaimsPrincipal user,
			SubmissionPermission permission,
			SubmissionAuthDto authData)
		{
			switch (permission)
			{
				case SubmissionPermission.Read:
				case SubmissionPermission.Download:
					// Authors and tournament managers
					// TODO: API key for workers
					return authData.AuthorId == userId ||
						authData.TournamentOwnerId == userId ||
						authData.TournamentManagersIds.Contains(userId);

				case SubmissionPermission.Update:
					// authors only
					return authData.AuthorId == userId;

				case SubmissionPermission.Search:
					return true; // TODO: More granular?

				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}