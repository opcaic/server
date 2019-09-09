using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Extensions;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;
using OPCAIC.Infrastructure.Repositories;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class SubmissionValidationPermissionHandler
		: ResourcePermissionAuthorizationHandler<SubmissionValidationPermission, SubmissionValidationAuthDto>
	{
		private readonly ISubmissionValidationRepository repository;

		public SubmissionValidationPermissionHandler(ISubmissionValidationRepository repository)
		{
			this.repository = repository;
		}

		/// <inheritdoc />
		protected override Task<SubmissionValidationAuthDto> GetAuthorizationData(long resourceId, CancellationToken cancellationToken = default)
		{
			return repository.GetAuthorizationData(resourceId, cancellationToken);
		}

		/// <inheritdoc />
		protected override bool HandlePermissionAsync(ClaimsPrincipal user, SubmissionValidationPermission permission,
			SubmissionValidationAuthDto authData)
		{
			switch (permission)
			{
				case SubmissionValidationPermission.UploadResult:
					// worker only
					return user.HasClaim(WorkerClaimTypes.ValidationId, authData.Id.ToString());

				case SubmissionValidationPermission.DownloadResult:
				case SubmissionValidationPermission.ReadDetail:
					var userId = user.TryGetId();
					// submission owners and tournament organizers
					return userId == authData.SubmissionAuthorId ||
						userId == authData.TournamentOwnerId ||
						authData.TournamentManagersIds.Contains(userId);
						
				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}