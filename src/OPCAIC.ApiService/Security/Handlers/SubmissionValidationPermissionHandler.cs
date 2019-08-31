using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Security.Handlers
{
	public class SubmissionValidationPermissionHandler
		: ResourcePermissionAuthorizationHandler<SubmissionValidationPermission, ResourceIdAuth>
	{
		/// <inheritdoc />
		protected override Task<ResourceIdAuth> GetAuthorizationData(long resourceId, CancellationToken cancellationToken = default)
		{
			return Task.FromResult(new ResourceIdAuth(resourceId));
		}

		/// <inheritdoc />
		protected override bool HandlePermissionAsync(ClaimsPrincipal user, SubmissionValidationPermission permission,
			ResourceIdAuth authData)
		{
			switch (permission)
			{
				case SubmissionValidationPermission.UploadResult:
					// worker only
					return user.HasClaim(WorkerClaimTypes.ValidationId, authData.Id.ToString());

				default:
					throw new ArgumentOutOfRangeException(nameof(permission), permission, null);
			}
		}
	}
}