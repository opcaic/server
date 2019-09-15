using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Extensions;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Interfaces.Repositories;

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
		protected override bool HandlePermissionAsync(ClaimsPrincipal user,
			SubmissionPermission permission,
			SubmissionAuthDto authData)
		{
			var userId = user.TryGetId();

			switch (permission)
			{
				case SubmissionPermission.Read:
				case SubmissionPermission.Download:
					// Authors tournament managers and workers
					return
						user.HasClaim(WorkerClaimTypes.SubmissionId, authData.Id.ToString()) ||
						authData.AuthorId == userId ||
						authData.TournamentOwnerId == userId ||
						authData.TournamentManagersIds.Contains(userId);

				case SubmissionPermission.Update:
					// authors only
					return authData.AuthorId == userId;

				case SubmissionPermission.Search:
					return true; // TODO: More granular?

				case SubmissionPermission.QueueValidation:
					return false; // admin only

				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}